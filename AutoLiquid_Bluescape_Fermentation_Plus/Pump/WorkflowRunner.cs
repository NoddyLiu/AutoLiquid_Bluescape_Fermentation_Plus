using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    // ============================================================
    // Step：sealed class 层级（对应 Android sealed class Step + 4 子类）
    // ============================================================

    /// <summary>
    /// 工作流步骤抽象基类（对应 Android 端 sealed class Step）。
    /// 4 个子类用嵌套 sealed class 表达，对应 Kotlin 的 4 个 sealed 子类。
    /// </summary>
    public abstract class Step
    {
        public string Note { get; protected set; }

        /// <summary>发送一条指令串（自动追加 R 由 WorkflowBuilder.ExecuteNow 控制）。</summary>
        public sealed class Send : Step
        {
            public string ChainAscii { get; }
            public Send(string chainAscii, string note = "")
            {
                ChainAscii = chainAscii;
                Note = note;
            }
        }

        /// <summary>延时。</summary>
        public sealed class Sleep : Step
        {
            public long Ms { get; }
            public Sleep(long ms, string note = "")
            {
                Ms = ms;
                Note = note;
            }
        }

        /// <summary>轮询 Q 命令直到 statusByte 空闲位 = 1 或超时。</summary>
        public sealed class AwaitIdle : Step
        {
            public long TimeoutMs { get; }
            public long PollMs { get; }
            public AwaitIdle(long timeoutMs, long pollMs)
            {
                TimeoutMs = timeoutMs;
                PollMs = pollMs;
                Note = "AwaitIdle";
            }
        }

        /// <summary>查一次状态字，错则抛错。带 3 次重试（工业现场 RS485 易丢包）。</summary>
        public sealed class EnsureNoError : Step
        {
            public EnsureNoError(string note = "")
            {
                Note = note;
            }
        }
    }

    // ============================================================
    // Sp1WorkflowDsl：DSL 注解（对应 Android @DslMarker + @Sp1WorkflowDsl）
    //
    // C# 没有 DslMarker 的"防止外层 lambda 调到内层 builder"的硬约束，
    // 这里仅作为"标记用"的 Attribute 保留语义。
    // ============================================================

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class Sp1WorkflowDslAttribute : Attribute
    {
    }

    // ============================================================
    // WorkflowBuilder：DSL 拼接器（对应 Android @Sp1WorkflowDsl class WorkflowBuilder）
    // ============================================================

    /// <summary>
    /// 工作流 DSL 拼接器。把所有 initZ/valveIn/pull/dispense/... 调用追加到内部 cmdBuf，
    /// 调 <see cref="ExecuteNow"/> 把 cmdBuf 转成 <see cref="Step.Send"/>；
    /// 直接调 <see cref="Sleep"/> / <see cref="AwaitIdle"/> / <see cref="EnsureNoError"/>
    /// 把对应的 Step 加入 steps 列表。
    ///
    /// 顶层入口：<see cref="Build(Action{WorkflowBuilder})"/> 静态方法，对应 Kotlin 的
    /// <c>workflow { ... }</c> 顶层函数。
    /// </summary>
    [Sp1WorkflowDsl]
    public sealed class WorkflowBuilder
    {
        private readonly List<Step> _steps = new List<Step>();
        private readonly StringBuilder _cmdBuf = new StringBuilder();

        public int StepCount { get { return _steps.Count; } }
        public int BufferLength { get { return _cmdBuf.Length; } }

        // -------- 命令拼接区：仅追加 ASCII 片段到 cmdBuf --------
        private void Add(string part) { _cmdBuf.Append(part); }

        public void Raw(string ascii) { Add(Sp1Cmd.Raw(ascii)); }

        // 初始化
        public void InitZ(int p) { Add(Sp1Cmd.InitZ(p)); }
        public void InitY(int p) { Add(Sp1Cmd.InitY(p)); }
        public void InitW(int p) { Add(Sp1Cmd.InitW(p)); }

        // 阀控制
        public void ValveIn() { Add(Sp1Cmd.ValveIn()); }
        public void ValveOut() { Add(Sp1Cmd.ValveOut()); }
        public void ValveBypass() { Add(Sp1Cmd.ValveBypass()); }
        public void ValveExtra() { Add(Sp1Cmd.ValveExtra()); }

        // 活塞
        public void MoveAbs(int steps) { Add(Sp1Cmd.MoveAbs(steps)); }
        public void Pull(int steps) { Add(Sp1Cmd.Pull(steps)); }
        public void Dispense(int steps) { Add(Sp1Cmd.Dispense(steps)); }

        // 速度 / 加减速 / 回退 / 微步
        public void SpeedStartHz(int hz) { Add(Sp1Cmd.SpeedStartHz(hz)); }
        public void SpeedRunHz(int hz) { Add(Sp1Cmd.SpeedRunHz(hz)); }
        public void SpeedStopHz(int hz) { Add(Sp1Cmd.SpeedStopHz(hz)); }
        public void SpeedLevel(int index) { Add(Sp1Cmd.SpeedTable(index)); }
        public void Slope(int n) { Add(Sp1Cmd.Slope(n)); }
        public void Backlash(int n) { Add(Sp1Cmd.Backlash(n)); }
        public void Microstep(int n) { Add(Sp1Cmd.Microstep(n)); }

        // 执行/控制
        public void LoopBegin() { Add(Sp1Cmd.LoopBegin()); }
        public void Loop(int times) { Add(Sp1Cmd.Loop(times)); }
        public void DelayCmd(int n) { Add(Sp1Cmd.Delay(n)); }
        public void Hold(int n) { Add(Sp1Cmd.Hold(n)); }
        public void PauseCmd() { Add(Sp1Cmd.Pause()); }
        public void ResumeCmd() { Add(Sp1Cmd.Resume()); }
        public void TerminateCmd() { Add(Sp1Cmd.Terminate()); }

        // EEPROM
        public void EepromSave(int slot) { Add(Sp1Cmd.EepromSave(slot)); }
        public void EepromExec(int slot) { Add(Sp1Cmd.EepromExec(slot)); }

        /// <summary>
        /// 把 cmdBuf 作为"发送步骤"加入流程并清空。autoAppendR=true 时自动在末尾追加 R。
        /// 与 Android 端 ExecuteNow 语义一致。
        /// </summary>
        public void ExecuteNow(string note = "", bool autoAppendR = true)
        {
            if (_cmdBuf.Length == 0) return;
            string chain = _cmdBuf.ToString();
            if (autoAppendR && !chain.EndsWith("R"))
                chain = chain + "R";
            _steps.Add(new Step.Send(chain, note));
            _cmdBuf.Clear();
        }

        // -------- 流程控制区：直接入 steps --------
        public void Sleep(long ms, string note = "")
        {
            _steps.Add(new Step.Sleep(ms, note));
        }

        public void AwaitIdle(long timeoutMs = 30000L, long pollMs = 200L)
        {
            _steps.Add(new Step.AwaitIdle(timeoutMs, pollMs));
        }

        public void EnsureNoError(string note = "")
        {
            _steps.Add(new Step.EnsureNoError(note));
        }

        /// <summary>生成只读的步骤列表（如果用户忘了 ExecuteNow，原 cmdBuf 内容会被丢弃）。</summary>
        internal IReadOnlyList<Step> BuildSteps()
        {
            return _steps.AsReadOnly();
        }

        /// <summary>
        /// 顶层入口：与 Kotlin 的 <c>workflow { ... }</c> 等价，直接拿到 List&lt;Step&gt;。
        /// </summary>
        public static IReadOnlyList<Step> Build(Action<WorkflowBuilder> block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var b = new WorkflowBuilder();
            block(b);
            return b.BuildSteps();
        }

        /**
         * 通用指令串
         */
        /**
         * 设置速度
         * @param speedLevel 速度等级 0~40
         */
        public void SetSpeed(int speedLevel)
        {
            SpeedLevel(speedLevel); 
            ExecuteNow();
            AwaitIdle();
            EnsureNoError();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            // 发 T，清除任何残留 hold/error 状态
            TerminateCmd();
            ExecuteNow("预清除终止");
            InitZ(1);
            ExecuteNow("初始化");
            // 等待空闲状态
           AwaitIdle();
            EnsureNoError("初始化检查");
        }

        /**
         * 抽液（相对位置）
         * @param steps 抽液步数
         * @param executeNote 执行动作打印信息
         * @param checkNote 错误检测信息
         */
        public void Pull(int steps, String executeNote = "", String checkNote = "")
        {
            ValveIn();
            Pull(steps);
            DelayCmd(200);
            ExecuteNow(executeNote);
            AwaitIdle();
            EnsureNoError(checkNote);
        }

        /**
         * 分液（相对位置）
         * @param steps 分液步数
         * @param executeNote 执行动作打印信息
         * @param checkNote 错误检测信息
         */
        public void Dispense(int steps, String executeNote = "", String checkNote = "")
        {
            ValveOut();
            Dispense(steps);
            DelayCmd(200);
            ExecuteNow(executeNote);
            AwaitIdle();
            EnsureNoError(checkNote);
        }

        /**
         * 分液（绝对位置）
         * @param steps 分液步数
         * @param executeNote 执行动作打印信息
         * @param checkNote 错误检测信息
         */
        public void DispenseAll(String executeNote = "", String checkNote = "")
        {
            ValveOut();
            MoveAbs(0);
            DelayCmd(200);
            ExecuteNow(executeNote);
            AwaitIdle();
            EnsureNoError(checkNote);
        }
    }

    // ============================================================
    // WorkflowRunner：步骤执行器（对应 Android class WorkflowRunner）
    // ============================================================

    /// <summary>
    /// 工作流执行器。
    ///
    /// 串行处理步骤列表中的每个 <see cref="Step"/>：
    /// - <c>Send</c>：调 <c>client.executeChain(...)</c>
    /// - <c>Sleep</c>：<c>Task.Delay</c>
    /// - <c>AwaitIdle</c>：轮询 Q 命令直到 isIdle=true 或超时
    /// - <c>EnsureNoError</c>：查一次状态字，错则抛错；3 次重试
    /// </summary>
    public sealed class WorkflowRunner
    {
        private readonly Sp1OemClient _client;
        private readonly Action<string> _log;

        public WorkflowRunner(Sp1OemClient client, Action<string> log = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
            _log = log ?? (delegate { });
        }

        public async Task RunAsync(IReadOnlyList<Step> steps, CancellationToken ct = default)
        {
            if (steps == null) return;
            for (int i = 0; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                Step s = steps[i];

                var sendStep = s as Step.Send;
                if (sendStep != null)
                {
                    _log("SEND " + sendStep.Note + ": " + sendStep.ChainAscii);
                    await _client.ExecuteChainAsync(sendStep.ChainAscii, true, null, ct).ConfigureAwait(false);
                    continue;
                }

                var sleepStep = s as Step.Sleep;
                if (sleepStep != null)
                {
                    _log("SLEEP " + sleepStep.Note + ": " + sleepStep.Ms + "ms");
                    await Task.Delay(TimeSpan.FromMilliseconds(sleepStep.Ms), ct).ConfigureAwait(false);
                    continue;
                }

                var idleStep = s as Step.AwaitIdle;
                if (idleStep != null)
                {
                    long startMs = NowMs();
                    while (NowMs() - startMs < idleStep.TimeoutMs)
                    {
                        ct.ThrowIfCancellationRequested();
                        var st = await _client.QueryStatusWordAsync(null, ct).ConfigureAwait(false);
                        if (st != null && st.IsIdle) break;
                        await Task.Delay(TimeSpan.FromMilliseconds(idleStep.PollMs), ct).ConfigureAwait(false);
                    }
                    continue;
                }

                var ensureStep = s as Step.EnsureNoError;
                if (ensureStep != null)
                {
                    Sp1StatusWord st = null;
                    // 加入重试机制：工业 RS485 总线在电机运行时容易受干扰丢包，给予 3 次重试机会
                    for (int r = 0; r < 3; r++)
                    {
                        st = await _client.QueryStatusWordAsync(null, ct).ConfigureAwait(false);
                        if (st != null) break;
                        await Task.Delay(TimeSpan.FromMilliseconds(300), ct).ConfigureAwait(false);
                    }
                    if (st == null)
                        throw new Exception("Q parse failed (" + ensureStep.Note + "：连续3次状态查询均超时无响应)");
                    if (st.ErrorCode != 0)
                        throw new Exception("Pump error=" + st.ErrorCode + " raw=" + st.Raw + " (" + ensureStep.Note + ")");
                    continue;
                }
            }
        }

        private static long NowMs()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }
}
