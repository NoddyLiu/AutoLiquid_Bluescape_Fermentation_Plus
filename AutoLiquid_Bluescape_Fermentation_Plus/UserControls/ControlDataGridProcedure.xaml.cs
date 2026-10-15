using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoLiquid_Bluescape_Fermentation_Plus.EntityJson;
using AutoLiquid_Bluescape_Fermentation_Plus.Mvvm;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.Window;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using AutoLiquid_Bluescape_Fermentation_Plus.MVVM;

namespace AutoLiquid_Bluescape_Fermentation_Plus.UserControls
{
    /// <summary>
    /// 程序DataGrid控件
    /// </summary>
    public partial class ControlDataGridProcedure : UserControl
    {
        // 程序步骤行index
        private int _stepRowIndex;

        // 程序model（用于显示界面）
        private ModelProcedure _modelProcedure;

        public ControlDataGridProcedure()
        {
            InitializeComponent();

            InitWidget();

            // 控件事件
            ControlEvent();
        }

        private void InitWidget()
        {
            _modelProcedure = new ModelProcedure();
            this.DataContext = _modelProcedure;
        }

        private void ControlEvent()
        {
            // 修改步骤
            this.DataGridDetail.SelectionChanged += DataGridProcedureOnSelectionChanged;
            this.DataGridDetail.LostFocus += DataGridProcedureOnLostFocus;
        }

        private void DataGridProcedureOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _stepRowIndex = this.DataGridDetail.SelectedIndex;
        }

        private void DataGridProcedureOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (_stepRowIndex == -1)
                return;

            var stepModel = _modelProcedure.ProcedureSteps[_stepRowIndex];
            var stepDetail = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].Steps[_stepRowIndex];

            SaveSelectedProcedure(stepModel, stepDetail);
        }

        private void SourceHolesOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var procedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex];
            var stepDetail = procedure.Steps[this._stepRowIndex];
            var stepModel = _modelProcedure.ProcedureSteps[_stepRowIndex];
            WindowHoleSelect whs = new WindowHoleSelect(ETemplateType.Source, stepDetail.SourceConsumable, stepDetail.SourceHoleIndexFrom, stepDetail.SourceHoleIndexTo, ConsumableHelper.GetConsumableType(0, stepDetail.SourceConsumable), stepDetail);
            var result = (bool)whs.ShowDialog();
            if (result)
            {
                stepModel.SourceHoleRange = ConsumableHelper.GetHoleString(stepDetail.HeadIndex, ConsumableHelper.GetConsumableType(stepDetail.HeadIndex, stepDetail.SourceConsumable), stepDetail.SourceHoleIndexFrom) +
                                            "~" + ConsumableHelper.GetHoleString(stepDetail.HeadIndex, ConsumableHelper.GetConsumableType(stepDetail.HeadIndex, stepDetail.SourceConsumable), stepDetail.SourceHoleIndexTo);
            }
        }

        private void TargetHolesOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var procedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex];
            var stepDetail = procedure.Steps[this._stepRowIndex];
            var stepModel = _modelProcedure.ProcedureSteps[_stepRowIndex];
            WindowHoleSelect whs = new WindowHoleSelect(ETemplateType.Target, stepDetail.TargetConsumable, stepDetail.TargetHoleIndexFrom, stepDetail.TargetHoleIndexTo, ConsumableHelper.GetConsumableType(0, stepDetail.TargetConsumable), stepDetail);
            var result = (bool)whs.ShowDialog();
            if (result)
            {
                stepModel.TargetHoleRange = ConsumableHelper.GetHoleString(stepDetail.HeadIndex, ConsumableHelper.GetConsumableType(stepDetail.HeadIndex, stepDetail.TargetConsumable), stepDetail.TargetHoleIndexFrom) +
                                            "~" + ConsumableHelper.GetHoleString(stepDetail.HeadIndex, ConsumableHelper.GetConsumableType(stepDetail.HeadIndex, stepDetail.TargetConsumable), stepDetail.TargetHoleIndexTo);
            }
        }

        private void BtnNewOnClick(object sender, RoutedEventArgs e)
        {
            var procedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex];
            /*
             * 在当前按钮行的下一行新增
             * 逻辑：如果步骤list长度 >= this._stepRowIndex + 1，则直接insert；否则需要add
             */
            var stepNew = ObjectUtils.DeepCopy(procedure.Steps[this._stepRowIndex]);
            stepNew.Name = (string)Application.Current.FindResource("ProcedureStep") + (procedure.Steps.Count + 1);
            var stepNewIndex = this._stepRowIndex + 1;
            if (stepNewIndex <= procedure.Steps.Count)
            {
                procedure.Steps.Insert(stepNewIndex, stepNew);
                InsertBindingStep(stepNewIndex, stepNew, new ModelColumnStep());
            }
            else
            {
                procedure.Steps.Add(stepNew);
                AddBindingStep(stepNew, new ModelColumnStep());
            }
            FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
        }

        private void BtnDelOnClick(object sender, RoutedEventArgs e)
        {
            var procedure = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex];
            var step = procedure.Steps[this._stepRowIndex];
            if (MessageBox.Show((string)this.FindResource("Prompt_If_Delete_Step_1") + step.Name + (string)this.FindResource("Prompt_If_Delete_Step_2"), (string)Application.Current.FindResource("Prompt"), MessageBoxButton.YesNo, MessageBoxImage.Warning) ==
                MessageBoxResult.Yes)
            {
                procedure.Steps.Remove(step);
                DelBindingStep(this._stepRowIndex);
                FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
            }
        }

        private void BtnUpOnClick(object sender, RoutedEventArgs e)
        {
            var currentIndex = this._stepRowIndex;
            var upIndex = currentIndex - 1;
            if (upIndex == -1)
                return;

            var procedureSteps = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].Steps;
            ObjectUtils.Swap(procedureSteps, currentIndex, upIndex);
            ObjectUtils.Swap(_modelProcedure.ProcedureSteps, currentIndex, upIndex);
            FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
        }

        private void BtnDownOnClick(object sender, RoutedEventArgs e)
        {
            var currentIndex = this._stepRowIndex;
            var downIndex = currentIndex + 1;
            if (downIndex == _modelProcedure.ProcedureSteps.Count)
                return;

            var procedureSteps = ParamsHelper.ProcedureAll.Procedures[ParamsHelper.ProcedureAll.ProcedureSelectedIndex].Steps;
            ObjectUtils.Swap(procedureSteps, currentIndex, downIndex);
            ObjectUtils.Swap(_modelProcedure.ProcedureSteps, currentIndex, downIndex);
            FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
        }

        /// <summary>
        /// 绑定选中的程序
        /// </summary>
        public void BindingSelectedProcedure()
        {
            // 清空binding
            _modelProcedure.ProcedureSteps.Clear();
            // 选中的程序所有步骤信息
            var procedureSelectedIndex = ParamsHelper.ProcedureAll.ProcedureSelectedIndex == -1 ? 0 : ParamsHelper.ProcedureAll.ProcedureSelectedIndex;
            foreach (var step in ParamsHelper.ProcedureAll.Procedures[procedureSelectedIndex].Steps)
            {
                AddBindingStep(step, new ModelColumnStep());
            }
        }

        /// <summary>
        /// 数据绑定赋值
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepModel"></param>
        private void BindingStepAssign(ProcedureStep step, ModelColumnStep stepModel)
        {
            stepModel.StepName = step.Name;
            stepModel.StepType = ObjectUtils.GetDescriptionFromEnum(step.StepType);
            stepModel.SourceTemplateTypeIndex = step.SourceTemplateTypeIndex;
            stepModel.SourceTemplate = step.SourceTemplateIndex + 1;
            //每次新增时候耗材会"" 在新增时做判断如果为""就默认增加一个指定耗材 下面也是如此
            if (step.SourceConsumable == "")
            {
                List<string> list = ParamsHelper.CommonSettingList[0].Consumables.Select(p => p.GroupName).ToList();
                step.SourceConsumable = list[0];
            }
            stepModel.SourceConsumable = step.SourceConsumable;
            stepModel.SourceTypeEditable = step.StepType == EStepType.LiquidTransfer || step.StepType == EStepType.TempFermentation;
            stepModel.SourceTemplateIndexEditable = step.StepType == EStepType.LiquidTransfer  || step.StepType == EStepType.TempFermentation || step.StepType == EStepType.ShakeStart || step.StepType == EStepType.ShakeStop;
            stepModel.SourceConsumableEditable = step.StepType == EStepType.LiquidTransfer || step.StepType == EStepType.TempFermentation;
            stepModel.SourceHoleRange = ConsumableHelper.GetHoleString(step.HeadIndex, ConsumableHelper.GetConsumableType(step.HeadIndex, step.SourceConsumable), step.SourceHoleIndexFrom) +
                                        "~" + ConsumableHelper.GetHoleString(step.HeadIndex, ConsumableHelper.GetConsumableType(step.HeadIndex, step.SourceConsumable), step.SourceHoleIndexTo);
            stepModel.SourceHoleRangeEditable = step.StepType == EStepType.LiquidTransfer;
            stepModel.TargetTemplateTypeIndex = step.TargetTemplateTypeIndex;
            stepModel.TargetTemplate = step.TargetTemplateIndex + 1;
            if (step.TargetConsumable == "")
            {
                List<string> list = ParamsHelper.CommonSettingList[0].Consumables.Select(p => p.GroupName).ToList();
                step.TargetConsumable = list[0];
            }
            stepModel.TargetConsumable = step.TargetConsumable;
            stepModel.TargetTemplateEditable = step.StepType == EStepType.LiquidTransfer || step.StepType == EStepType.TempSample;
            stepModel.TargetHoleRange = ConsumableHelper.GetHoleString(step.HeadIndex, ConsumableHelper.GetConsumableType(step.HeadIndex, step.TargetConsumable), step.TargetHoleIndexFrom) +
                                        "~" + ConsumableHelper.GetHoleString(step.HeadIndex, ConsumableHelper.GetConsumableType(step.HeadIndex, step.TargetConsumable), step.TargetHoleIndexTo);
            stepModel.TargetHoleRangeEditable = step.StepType == EStepType.LiquidTransfer;
            stepModel.VolumeEach = step.VolumeEach;
            stepModel.IsNeedCleaning = step.IsNeedCleaning;
            stepModel.CleaningPosIndexArray[0] = step.CleaningPosIndexArray[0];
            stepModel.CleaningPosIndexArray[1] = step.CleaningPosIndexArray[1];
            stepModel.CleaningPosIndexArray[2] = step.CleaningPosIndexArray[2];
            //stepModel.AbsorbMixingVolume = step.AbsorbMixingVolume;
            //stepModel.AbsorbMixingTimes = step.AbsorbMixingTimes;
            //stepModel.AbsorbMixingEditable = step.StepType == EStepType.LiquidTransfer;
            //stepModel.JetMixingVolume = step.JetMixingVolume;
            //stepModel.JetMixingTimes = step.JetMixingTimes;
            //stepModel.JetMixingEditable = step.StepType == EStepType.LiquidTransfer;
            stepModel.Temp = step.Temp;
            stepModel.TempEditable = step.StepType == EStepType.TempFermentation || step.StepType == EStepType.TempSample;
            //stepModel.PreTemp = step.PreTemp;
            //stepModel.PreTempEditable = step.StepType == EStepType.Heat;
            stepModel.LoopStepRange[0] = step.LoopStepRange[0];
            stepModel.LoopStepRange[1] = step.LoopStepRange[1];
            stepModel.LoopTimes = step.LoopTimes;
            stepModel.LoopEditable = step.StepType == EStepType.Loop;
            stepModel.Time = step.Time;
            stepModel.TimeEditable = step.StepType == EStepType.ShakeStart || step.StepType == EStepType.TempFermentation || step.StepType == EStepType.TempSample || step.StepType == EStepType.Wait;
            stepModel.Block = step.Block;
            stepModel.BlockEditable = step.StepType == EStepType.Wait;
            stepModel.WaitForOrderList = Enumerable.Range(1, _modelProcedure.ProcedureSteps.Count).ToList();
            stepModel.WaitForOrder = step.WaitForOrder;
            stepModel.WaitForOrderEditable = step.StepType == EStepType.Wait && !stepModel.Block;
            stepModel.SpeedLevel = step.SpeedLevel;
            stepModel.SpeedLevelEditable = step.StepType == EStepType.ShakeStart;
        }

        /// <summary>
        /// 数据绑定：添加步骤
        /// </summary>
        /// <param name="step"></param>
        /// <param name="stepModel"></param>
        private void AddBindingStep(ProcedureStep step, ModelColumnStep stepModel)
        {
            BindingStepAssign(step, stepModel);
            _modelProcedure.ProcedureSteps.Add(stepModel);
        }

        /// <summary>
        /// 数据绑定：删除步骤
        /// </summary>
        /// <param name="index"></param>
        private void DelBindingStep(int index)
        {
            _modelProcedure.ProcedureSteps.RemoveAt(index);
        }

        /// <summary>
        /// 数据绑定：插入步骤
        /// </summary>
        /// <param name="insertIndex"></param>
        /// <param name="step"></param>
        /// <param name="stepModel"></param>
        private void InsertBindingStep(int insertIndex, ProcedureStep step, ModelColumnStep stepModel)
        {
            BindingStepAssign(step, stepModel);
            _modelProcedure.ProcedureSteps.Insert(insertIndex, stepModel);
        }

        /// <summary>
        /// 保存修改的程序
        /// </summary>
        /// <param name="stepModel"></param>
        /// <param name="stepDetail"></param>
        private void SaveSelectedProcedure(ModelColumnStep stepModel, ProcedureStep stepDetail)
        {
            // 是否需要更新
            var needUpdate = false;

            // 步骤名称
            if (stepDetail.Name != stepModel.StepName)
            {
                needUpdate = true;
                stepDetail.Name = stepModel.StepName;
            }
            // 步骤类型
            if (ObjectUtils.GetDescriptionFromEnum(stepDetail.StepType) != stepModel.StepType)
            {
                needUpdate = true;
                stepDetail.StepType = ObjectUtils.GetEnumFromDescription<EStepType>(stepModel.StepType);
            }
            /**
             * 源盘
             */
            if (stepDetail.SourceTemplateTypeIndex != stepModel.SourceTemplateTypeIndex)
            {
                needUpdate = true;
                stepDetail.SourceTemplateTypeIndex = stepModel.SourceTemplateTypeIndex;
            }
            if (stepDetail.SourceTemplateIndex + 1 != stepModel.SourceTemplate)
            {
                needUpdate = true;
                stepDetail.SourceTemplateIndex = stepModel.SourceTemplate - 1;
            }
            if (stepDetail.SourceConsumable != stepModel.SourceConsumable)
            {
                needUpdate = true;
                stepDetail.SourceConsumable = stepModel.SourceConsumable;
            }
            /**
             * 靶盘
             */
            if (stepDetail.TargetTemplateTypeIndex != stepModel.TargetTemplateTypeIndex)
            {
                needUpdate = true;
                stepDetail.TargetTemplateTypeIndex = stepModel.TargetTemplateTypeIndex;
            }
            if (stepDetail.TargetTemplateIndex + 1 != stepModel.TargetTemplate)
            {
                needUpdate = true;
                stepDetail.TargetTemplateIndex = stepModel.TargetTemplate - 1;
            }
            if (stepDetail.TargetConsumable != stepModel.TargetConsumable)
            {
                needUpdate = true;
                stepDetail.TargetConsumable = stepModel.TargetConsumable;
            }

            if (stepDetail.VolumeEach != stepModel.VolumeEach)
            {
                needUpdate = true;
                stepDetail.VolumeEach = stepModel.VolumeEach;
            }

            /**
             * 清洗区
             */
            if (stepDetail.IsNeedCleaning != stepModel.IsNeedCleaning)
            {
                needUpdate = true;
                stepDetail.IsNeedCleaning = stepModel.IsNeedCleaning;
            }
            if (stepDetail.CleaningPosIndexArray[0] != stepModel.CleaningPosIndexArray[0])
            {
                needUpdate = true;
                if (stepModel.CleaningPosIndexArray[0] == 6)
                    stepDetail.CleaningPosIndexArray[0] = -1;
                else
                    stepDetail.CleaningPosIndexArray[0] = stepModel.CleaningPosIndexArray[0];
            }
            if (stepDetail.CleaningPosIndexArray[1] != stepModel.CleaningPosIndexArray[1])
            {
                needUpdate = true;
                if (stepModel.CleaningPosIndexArray[1] == 6)
                    stepDetail.CleaningPosIndexArray[1] = -1;
                else
                    stepDetail.CleaningPosIndexArray[1] = stepModel.CleaningPosIndexArray[1];
            }
            if (stepDetail.CleaningPosIndexArray[2] != stepModel.CleaningPosIndexArray[2])
            {
                needUpdate = true;
                if (stepModel.CleaningPosIndexArray[2] == 6)
                    stepDetail.CleaningPosIndexArray[2] = -1;
                else
                    stepDetail.CleaningPosIndexArray[2] = stepModel.CleaningPosIndexArray[2];
            }

            /**
             * 吸前混合
             */
            //if (stepDetail.AbsorbMixingVolume != stepModel.AbsorbMixingVolume)
            //{
            //    needUpdate = true;
            //    stepDetail.AbsorbMixingVolume = stepModel.AbsorbMixingVolume;
            //}
            //if (stepDetail.AbsorbMixingTimes != stepModel.AbsorbMixingTimes)
            //{
            //    needUpdate = true;
            //    stepDetail.AbsorbMixingTimes = stepModel.AbsorbMixingTimes;
            //}
            /**
             * 喷后混合
             */
            //if (stepDetail.JetMixingVolume != stepModel.JetMixingVolume)
            //{
            //    needUpdate = true;
            //    stepDetail.JetMixingVolume = stepModel.JetMixingVolume;
            //}
            //if (stepDetail.JetMixingTimes != stepModel.JetMixingTimes)
            //{
            //    needUpdate = true;
            //    stepDetail.JetMixingTimes = stepModel.JetMixingTimes;
            //}

            /**
             * 循环
             */
            if (stepDetail.LoopStepRange[0] != stepModel.LoopStepRange[0])
            {
                needUpdate = true;
                stepDetail.LoopStepRange[0] = stepModel.LoopStepRange[0];
            }
            if (stepDetail.LoopStepRange[1] != stepModel.LoopStepRange[1])
            {
                if (stepModel.LoopStepRange[1] < stepModel.LoopStepRange[0])
                    MessageBox.Show((string)this.FindResource("Prompt_StopPos_Must_Great_Or_Equal_To_StartPos"));
                else
                {
                    needUpdate = true;
                    stepDetail.LoopStepRange[1] = stepModel.LoopStepRange[1];
                }
            }
            if (stepDetail.LoopTimes != stepModel.LoopTimes)
            {
                needUpdate = true;
                stepDetail.LoopTimes = stepModel.LoopTimes;
            }

            // 时间
            if (stepDetail.Time != stepModel.Time)
            {
                needUpdate = true;
                stepDetail.Time = stepModel.Time;
            }
            // 阻塞
            if (stepDetail.Block != stepModel.Block)
            {
                needUpdate = true;
                stepDetail.Block = stepModel.Block;
            }
            // 等待序号
            if (stepDetail.WaitForOrder != stepModel.WaitForOrder)
            {
                needUpdate = true;
                stepDetail.WaitForOrder = stepModel.WaitForOrder;
            }
            // 温度
            if (stepDetail.Temp != stepModel.Temp)
            {
                needUpdate = true;
                stepDetail.Temp = stepModel.Temp;
            }
            // 预热
            //if (stepDetail.PreTemp != stepModel.PreTemp)
            //{
            //    needUpdate = true;
            //    stepDetail.PreTemp = stepModel.PreTemp;
            //}
            // 速度
            if (stepDetail.SpeedLevel != stepModel.SpeedLevel)
            {
                needUpdate = true;
                stepDetail.SpeedLevel = stepModel.SpeedLevel;
            }

            if (needUpdate)
                FileUtils.SaveProcedureAll(ParamsHelper.ProcedureAll);
        }


    }
}
