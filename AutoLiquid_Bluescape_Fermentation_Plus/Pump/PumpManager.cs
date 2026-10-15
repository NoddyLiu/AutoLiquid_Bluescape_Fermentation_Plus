using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    // ============================================================
    // PumpId 枚举 + 扩展（对应 Android enum class PumpId + 扩展方法）
    // ============================================================

    /// <summary>
    /// 泵 ID 枚举（与 Android 端 PumpId 对齐）。
    /// 地址字符：LEFT='1' / RIGHT='2'。
    /// </summary>
    public enum PumpId
    {
        Left = 1,
        Right = 2,
    }

    public static class PumpIdExtensions
    {
        public const char LeftAddr = '1';
        public const char RightAddr = '2';

        public static char ToAddr(this PumpId id)
        {
            switch (id)
            {
                case PumpId.Left: return LeftAddr;
                case PumpId.Right: return RightAddr;
                default: throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        public static string DisplayName(this PumpId id)
        {
            switch (id)
            {
                case PumpId.Left: return "左泵";
                case PumpId.Right: return "右泵";
                default: return id.ToString();
            }
        }

        public static PumpId? FromAddress(char addr)
        {
            if (addr == LeftAddr) return PumpId.Left;
            if (addr == RightAddr) return PumpId.Right;
            return null;
        }
    }

    // ============================================================
    // PumpSelection sealed 层级（对应 Android sealed class PumpSelection）
    // ============================================================

    /// <summary>
    /// 泵选择模式（对应 Android 端 sealed class PumpSelection）。
    /// 用抽象基类 + 三个嵌套 sealed 子类 + 三个静态只读实例表达。
    /// </summary>
    public abstract class PumpSelection
    {
        public abstract IReadOnlyList<PumpId> GetTargetPumps();

        public sealed class LeftSel : PumpSelection
        {
            public override IReadOnlyList<PumpId> GetTargetPumps()
            {
                return new PumpId[] { PumpId.Left };
            }
        }

        public sealed class RightSel : PumpSelection
        {
            public override IReadOnlyList<PumpId> GetTargetPumps()
            {
                return new PumpId[] { PumpId.Right };
            }
        }

        public sealed class BothSel : PumpSelection
        {
            public override IReadOnlyList<PumpId> GetTargetPumps()
            {
                return new PumpId[] { PumpId.Left, PumpId.Right };
            }
        }

        public static readonly PumpSelection Left = new LeftSel();
        public static readonly PumpSelection Right = new RightSel();
        public static readonly PumpSelection Both = new BothSel();
    }

    // ============================================================
    // PumpManager（对应 Android class PumpManager）
    // ============================================================

    /// <summary>
    /// 泵管理器（对应 Android 端 PumpManager.kt）。
    ///
    /// 行为对齐：
    /// - 所有泵共享一条 RS485 总线，必须共享同一把 <c>busMutex</c>，
    ///   防止多泵并行时帧碰撞。
    /// - 同一串口只需 open / close 一次。
    /// - 提供唯一的 <see cref="RunWorkflowAsync(PumpSelection, Action{WorkflowContext}, CancellationToken)"/>
    ///   入口，对应 Android 端的 <c>runWorkflow(selection, workflow: WorkflowContext.() -> Unit)</c>。
    ///   selection 决定并行/单泵/双泵语义，DSL 用 <see cref="WorkflowContext"/> 编排。
    /// </summary>
    public sealed class PumpManager
    {
        private readonly ITransport _transport;
        private readonly Action<string> _log;
        private readonly SemaphoreSlim _busMutex = new SemaphoreSlim(1, 1);
        private readonly Dictionary<PumpId, PumpInstance> _pumps = new Dictionary<PumpId, PumpInstance>();

        public PumpManager(ITransport transport, Action<string> log = null)
        {
            if (transport == null) throw new ArgumentNullException(nameof(transport));
            _transport = transport;
            _log = log ?? (delegate { });
        }

        /// <summary>注册泵实例（绑定对应地址 + 共享总线互斥）。</summary>
        public void RegisterPump(PumpId id)
        {
            char addr = id.ToAddr();
            var client = new Sp1OemClient(_transport, addr, _busMutex);
            var runner = new WorkflowRunner(client, msg => _log("[" + id.DisplayName() + "] " + msg));
            _pumps[id] = new PumpInstance(client, runner);
        }

        /// <summary>同一串口只需打开一次。</summary>
        public async Task OpenAllAsync(CancellationToken ct = default)
        {
            if (_pumps.Count == 0) return;
            Sp1OemClient firstClient = null;
            foreach (var kv in _pumps) { firstClient = kv.Value.Client; break; }
            if (firstClient != null) await firstClient.OpenAsync(ct).ConfigureAwait(false);
        }

        public async Task CloseAllAsync(CancellationToken ct = default)
        {
            if (_pumps.Count == 0) return;
            Sp1OemClient firstClient = null;
            foreach (var kv in _pumps) { firstClient = kv.Value.Client; break; }
            if (firstClient != null) await firstClient.CloseAsync(ct).ConfigureAwait(false);
        }

        public Sp1OemClient GetClient(PumpId id)
        {
            PumpInstance p;
            return _pumps.TryGetValue(id, out p) ? p.Client : null;
        }

        public WorkflowRunner GetRunner(PumpId id)
        {
            PumpInstance p;
            return _pumps.TryGetValue(id, out p) ? p.Runner : null;
        }

        public bool IsPumpRegistered(PumpId id)
        {
            return _pumps.ContainsKey(id);
        }

        public IReadOnlyCollection<PumpId> GetRegisteredPumps()
        {
            return _pumps.Keys;
        }

        /// <summary>
        /// 唯一的工作流执行入口（对应 Android 端的 <c>runWorkflow(selection, workflow: WorkflowContext.() -> Unit)</c>）。
        /// selection 决定哪些泵被驱动；<paramref name="block"/> 在 <see cref="WorkflowContext"/> 上编排任务。
        /// </summary>
        public async Task RunWorkflowAsync(PumpSelection selection, Action<WorkflowContext> block, CancellationToken ct = default)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));
            if (block == null) throw new ArgumentNullException(nameof(block));
            var ctx = new WorkflowContext(this, selection);
            block(ctx);
            await ctx.ExecuteAsync(ct).ConfigureAwait(false);
        }

        private struct PumpInstance
        {
            public Sp1OemClient Client { get; }
            public WorkflowRunner Runner { get; }
            public PumpInstance(Sp1OemClient client, WorkflowRunner runner)
            {
                Client = client;
                Runner = runner;
            }
        }
    }

    // ============================================================
    // WorkflowContext（对应 Android class WorkflowContext）
    // ============================================================

    /// <summary>
    /// 工作流上下文 / DSL 容器（对应 Android 端 WorkflowContext）。
    ///
    /// 暴露的 DSL 方法：
    /// - <see cref="Parallel"/>：对 selection 中每台泵**并发**执行相同 steps
    /// - <see cref="Sequential"/>：对 selection 中每台泵**顺序**执行相同 steps
    /// - <see cref="Pump"/> / <see cref="LeftPump"/> / <see cref="RightPump"/>：只针对一台泵
    /// - <see cref="Alternate"/>：左右泵**交替**执行 N 轮（左分+右吸 / 右分+左吸）
    /// - <see cref="Sleep"/>：延时
    ///
    /// 内部用 <see cref="WorkflowTask"/> 列表收集所有任务；<see cref="ExecuteAsync"/> 时统一按类型执行。
    /// </summary>
    public sealed class WorkflowContext
    {
        private readonly PumpManager _manager;
        private readonly PumpSelection _selection;
        private readonly List<WorkflowTask> _tasks = new List<WorkflowTask>();

        public WorkflowContext(PumpManager manager, PumpSelection selection)
        {
            _manager = manager;
            _selection = selection;
        }

        /// <summary>对 selection 中每台泵并发执行相同 steps（busMutex 串行化物理帧）。</summary>
        public void Parallel(string note, Action<WorkflowBuilder> block)
        {
            _tasks.Add(new WorkflowTask.Parallel(_selection, WorkflowBuilder.Build(block), note));
        }

        /// <summary>对 selection 中每台泵顺序执行相同 steps（按 selection 顺序）。</summary>
        public void Sequential(string note, Action<WorkflowBuilder> block)
        {
            _tasks.Add(new WorkflowTask.Sequential(_selection, WorkflowBuilder.Build(block), note));
        }

        /// <summary>只针对一台泵（无视当前 selection）。</summary>
        public void Pump(PumpId id, string note, Action<WorkflowBuilder> block)
        {
            _tasks.Add(new WorkflowTask.Specific(id, WorkflowBuilder.Build(block), note));
        }

        public void LeftPump(string note, Action<WorkflowBuilder> block)
        {
            Pump(PumpId.Left, note, block);
        }

        public void RightPump(string note, Action<WorkflowBuilder> block)
        {
            Pump(PumpId.Right, note, block);
        }

        /// <summary>
        /// 左右泵交替执行 N 轮；每轮：左右泵并发执行各自一组 steps。
        /// 经典用例：左泵分液+右泵吸液 → 左泵吸液+右泵分液。
        /// </summary>
        public void Alternate(int cycles, string note, Action<AlternateBuilder> block)
        {
            var b = new AlternateBuilder();
            block(b);
            _tasks.Add(new WorkflowTask.Alternate(b.LeftSteps, b.RightSteps, cycles, note));
        }

        public void Sleep(long ms, string note = "")
        {
            _tasks.Add(new WorkflowTask.Sleep(ms, note));
        }

        /// <summary>按收集顺序执行所有任务。</summary>
        internal async Task ExecuteAsync(CancellationToken ct = default)
        {
            for (int t = 0; t < _tasks.Count; t++)
            {
                ct.ThrowIfCancellationRequested();
                var task = _tasks[t];

                if (task is WorkflowTask.Parallel p)
                {
                    var targets = p.Selection.GetTargetPumps();
                    var ts = new List<Task>();
                    foreach (var pid in targets)
                    {
                        var runner = _manager.GetRunner(pid);
                        if (runner == null) continue;
                        var steps = p.Steps;
                        ts.Add(Task.Run(async () => await runner.RunAsync(steps, ct).ConfigureAwait(false), ct));
                    }
                    if (ts.Count > 0) await Task.WhenAll(ts).ConfigureAwait(false);
                }
                else if (task is WorkflowTask.Sequential s)
                {
                    var targets = s.Selection.GetTargetPumps();
                    for (int i = 0; i < targets.Count; i++)
                    {
                        var runner = _manager.GetRunner(targets[i]);
                        if (runner == null) continue;
                        await runner.RunAsync(s.Steps, ct).ConfigureAwait(false);
                    }
                }
                else if (task is WorkflowTask.Specific sp)
                {
                    var runner = _manager.GetRunner(sp.PumpId);
                    if (runner != null) await runner.RunAsync(sp.Steps, ct).ConfigureAwait(false);
                }
                else if (task is WorkflowTask.Alternate alt)
                {
                    for (int c = 0; c < alt.Cycles; c++)
                    {
                        ct.ThrowIfCancellationRequested();
                        var left = _manager.GetRunner(PumpId.Left);
                        var right = _manager.GetRunner(PumpId.Right);
                        var pair = new List<Task>();
                        if (left != null) pair.Add(Task.Run(async () => await left.RunAsync(alt.LeftSteps, ct).ConfigureAwait(false), ct));
                        if (right != null) pair.Add(Task.Run(async () => await right.RunAsync(alt.RightSteps, ct).ConfigureAwait(false), ct));
                        if (pair.Count > 0) await Task.WhenAll(pair).ConfigureAwait(false);
                    }
                }
                else if (task is WorkflowTask.Sleep sl)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(sl.Ms), ct).ConfigureAwait(false);
                }
            }
        }
    }

    // ============================================================
    // AlternateBuilder（对应 Android class AlternateBuilder）
    // ============================================================

    /// <summary>
    /// Alternate 任务的构建器（对应 Android 端 AlternateBuilder）。
    /// 分别构造左泵和右泵各自的一组 steps。
    /// </summary>
    public sealed class AlternateBuilder
    {
        public IReadOnlyList<Step> LeftSteps { get; private set; }
        public IReadOnlyList<Step> RightSteps { get; private set; }

        public void LeftPump(string note, Action<WorkflowBuilder> block)
        {
            LeftSteps = WorkflowBuilder.Build(block);
        }

        public void RightPump(string note, Action<WorkflowBuilder> block)
        {
            RightSteps = WorkflowBuilder.Build(block);
        }
    }

    // ============================================================
    // WorkflowTask（对应 Android sealed class WorkflowTask + 5 子类）
    // ============================================================

    /// <summary>
    /// 工作流任务类型（对应 Android 端 sealed class WorkflowTask）。
    /// 5 个嵌套子类对应 Parallel / Sequential / Specific / Alternate / Sleep。
    /// </summary>
    internal abstract class WorkflowTask
    {
        public sealed class Parallel : WorkflowTask
        {
            public PumpSelection Selection { get; }
            public IReadOnlyList<Step> Steps { get; }
            public string Note { get; }
            public Parallel(PumpSelection selection, IReadOnlyList<Step> steps, string note)
            {
                Selection = selection;
                Steps = steps;
                Note = note;
            }
        }

        public sealed class Sequential : WorkflowTask
        {
            public PumpSelection Selection { get; }
            public IReadOnlyList<Step> Steps { get; }
            public string Note { get; }
            public Sequential(PumpSelection selection, IReadOnlyList<Step> steps, string note)
            {
                Selection = selection;
                Steps = steps;
                Note = note;
            }
        }

        public sealed class Specific : WorkflowTask
        {
            public PumpId PumpId { get; }
            public IReadOnlyList<Step> Steps { get; }
            public string Note { get; }
            public Specific(PumpId pumpId, IReadOnlyList<Step> steps, string note)
            {
                PumpId = pumpId;
                Steps = steps;
                Note = note;
            }
        }

        public sealed class Alternate : WorkflowTask
        {
            public IReadOnlyList<Step> LeftSteps { get; }
            public IReadOnlyList<Step> RightSteps { get; }
            public int Cycles { get; }
            public string Note { get; }
            public Alternate(IReadOnlyList<Step> leftSteps, IReadOnlyList<Step> rightSteps, int cycles, string note)
            {
                LeftSteps = leftSteps;
                RightSteps = rightSteps;
                Cycles = cycles;
                Note = note;
            }
        }

        public sealed class Sleep : WorkflowTask
        {
            public long Ms { get; }
            public string Note { get; }
            public Sleep(long ms, string note)
            {
                Ms = ms;
                Note = note;
            }
        }
    }
}
