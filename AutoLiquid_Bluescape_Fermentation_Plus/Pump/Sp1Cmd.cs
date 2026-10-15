using System;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Pump
{
    /// <summary>
    /// OEM 协议常量 —— 步进上限（与 Android 端 ConstantsUtils.STEP_LIMIT 对齐）。
    /// 与 <see cref="Sp1Cmd"/> 同文件，呼应 Android 端 Sp1Cmd.kt 的内嵌结构。
    /// </summary>
    public static class Sp1CmdConst
    {
        public const int StepLimit = 6000;
    }

    /// <summary>
    /// SP1 命令片段构造器（对应 Android 端 Sp1Cmd.kt）。
    ///
    /// 所有方法返回 ASCII 字符串片段，可被 WorkflowBuilder 拼到指令串中，
    /// 末尾追加 'R' 即执行，例如 initZ(1) + speedV(2000) + moveAbs(6000) + valveOut()。
    /// </summary>
    public static class Sp1Cmd
    {
        // -------- 执行/控制（指令串相关） --------
        public static string Run() { return "R"; }
        public static string RunAgain() { return "X"; }
        public static string Pause() { return "h"; }
        public static string Resume() { return "r"; }
        public static string Loop(int times) { return "G" + times; }      // times=0 表示无限循环
        public static string LoopBegin() { return "g"; }
        public static string Delay(int ms) { return "M" + ms; }
        public static string Hold(int n) { return "H" + n; }
        public static string Terminate() { return "T"; }                  // 终止活塞相关运动

        // ---- 活塞运动 ----
        // A<n> 范围 0..6000（部分机器可到 6150）
        public static string MoveAbs(int pos)
        {
            if (pos < 0 || pos > 6150) throw new ArgumentOutOfRangeException(nameof(pos), "pos 必须在 0..6150");
            return "A" + pos;
        }
        // P<n> 向下/抽取
        public static string Pull(int steps)
        {
            if (steps < 0 || steps > Sp1CmdConst.StepLimit)
                throw new ArgumentOutOfRangeException(nameof(steps), "steps 必须在 0.." + Sp1CmdConst.StepLimit);
            return "P" + steps;
        }
        // D<n> 向上/分配
        public static string Dispense(int steps)
        {
            if (steps < 0 || steps > Sp1CmdConst.StepLimit)
                throw new ArgumentOutOfRangeException(nameof(steps), "steps 必须在 0.." + Sp1CmdConst.StepLimit);
            return "D" + steps;
        }

        // ---- 阀控制 ----
        public static string ValveIn() { return "I"; }
        public static string ValveOut() { return "O"; }
        public static string ValveBypass() { return "B"; }
        public static string ValveExtra() { return "E"; }

        // ---- 初始化 ----
        public static string InitZ(int n) { return "Z" + n; }
        public static string InitY(int n) { return "Y" + n; }
        public static string InitW(int n) { return "W" + n; }

        // ---- 速度 / 加减速 / 回退 / 微步 ----
        public static string SpeedStartHz(int hz) { return "v" + hz; }
        public static string SpeedRunHz(int hz) { return "V" + hz; }
        public static string SpeedStopHz(int hz) { return "c" + hz; }
        public static string SpeedTable(int index) { return "S" + index; }   // 0..40
        public static string Slope(int index) { return "L" + index; }       // 1..20
        public static string Backlash(int steps) { return "K" + steps; }    // 0..31
        public static string Microstep(int mode) { return "N" + mode; }      // 0=整步 / 1=微步

        // ---- 外控输出（OC 输出） ----
        public static string OutMask(int mask) { return "J" + mask; }        // 0..7

        // ---- EEPROM 指令串（0..14） ----
        public static string EepromSave(int slot) { return "s" + slot; }
        public static string EepromExec(int slot) { return "e" + slot; }

        // ---- 报告/查询 ----
        public static string QTargetPos() { return "?"; }
        public static string QStartSpeed() { return "?1"; }
        public static string QRunSpeed() { return "?2"; }
        public static string QStopSpeed() { return "?3"; }
        public static string QCurrentPos() { return "?4"; }
        public static string QBufferState() { return "?10"; }
        public static string QBacklash() { return "?12"; }
        public static string QPumpAddr() { return "?15"; }
        public static string QErrorCode() { return "?16"; }
        public static string QVersion() { return "?23"; }
        public static string QErrorAndStatusWord() { return "Q"; }

        /// <summary>原样追加（调试时用，不要绕过 range check）。</summary>
        public static string Raw(string ascii) { return ascii; }
    }
}
