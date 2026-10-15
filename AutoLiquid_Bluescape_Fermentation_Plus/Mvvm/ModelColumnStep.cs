using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AutoLiquid_Bluescape_Fermentation_Plus.Utils;
using AutoLiquid_Library.Enum;
using AutoLiquid_Library.Utils;
using ConstantsUtils = AutoLiquid_Bluescape_Fermentation_Plus.Utils.ConstantsUtils;

namespace AutoLiquid_Bluescape_Fermentation_Plus.Mvvm
{
    /// <summary>
    /// 步骤信息列名 Model
    /// </summary>
    public class ModelColumnStep : INotifyPropertyChanged
    {
        // 步骤名称
        private string _stepName = string.Empty;
        public string StepName
        {
            set => UpdateProperty(ref _stepName, value);
            get => _stepName;
        }

        // 步骤类型
        private List<string> _stepTypeList = new List<string>();
        public List<string> StepTypeList
        {
            get
            {
                _stepTypeList.Clear();
                _stepTypeList.AddRange(new List<string>
                {
                    ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer),
                    ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStart),
                    ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStop),
                    ObjectUtils.GetDescriptionFromEnum(EStepType.Cleaning),
                    ObjectUtils.GetDescriptionFromEnum(EStepType.Wait),
                    ObjectUtils.GetDescriptionFromEnum(EStepType.Loop),
                    // ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation),
                    // ObjectUtils.GetDescriptionFromEnum(EStepType.TempSample),
                });
                return _stepTypeList;
            }
        }
        // 选择的步骤类型
        private string _stepType = string.Empty;
        public string StepType
        {
            set
            {
                UpdateProperty(ref _stepType, value);
                // 根据步骤类型禁用相关列
                VolumeEachEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer);
                SourceTypeEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation);
                SourceTemplateIndexEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStart) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStop);
                SourceConsumableEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation);
                SourceHoleRangeEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer);
                TargetTemplateEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempSample);
                TargetHoleRangeEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer);
                CleaningEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.LiquidTransfer) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Cleaning);
                LoopEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Loop);
                TempEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempSample);
                TimeEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStart) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempFermentation) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.TempSample) || _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Wait);
                SpeedLevelEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.ShakeStart);
                BlockEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Wait);
                WaitForOrderEditable = _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Wait) && !Block;
            }
            get => _stepType;
        }

        /**
         * 源盘
         */
        // 盘位类型Index（发酵区 或 留样区）
        private int _sourceTemplateTypeIndex = 0;
        public int SourceTemplateTypeIndex
        {
            set => UpdateProperty(ref _sourceTemplateTypeIndex, value);
            get => _sourceTemplateTypeIndex;
        }
        // 盘位可用List
        private List<int> _sourceTemplateList = new List<int>();
        public List<int> SourceTemplateList
        {
            get
            {
                _sourceTemplateList.Clear();
                _sourceTemplateList.AddRange(new List<int> { 1, 2, 3, 4 });
                return _sourceTemplateList;
            }
        }
        // 选择的盘位
        private int _sourceTemplate = 0;
        public int SourceTemplate
        {
            set => UpdateProperty(ref _sourceTemplate, value);
            get => _sourceTemplate;
        }
        // 耗材
        private List<string> _sourceConsumableList = new List<string>();
        public List<string> SourceConsumableList
        {
            get
            {
                _sourceConsumableList.Clear();
                _sourceConsumableList.AddRange(ParamsHelper.CommonSettingList[0].Consumables.Select(p => p.GroupName).ToList());
                return _sourceConsumableList;
            }
        }
        // 源盘耗材
        private string _sourceConsumable = string.Empty;
        public string SourceConsumable
        {
            set => UpdateProperty(ref _sourceConsumable, value);
            get => _sourceConsumable;
        }
        // 源盘孔范围
        private string _sourceHoleRange = string.Empty;
        public string SourceHoleRange
        {
            set => UpdateProperty(ref _sourceHoleRange, value);
            get => _sourceHoleRange;
        }
        // 源盘类型是否可编辑
        private bool _sourceTypeEditable = true;
        public bool SourceTypeEditable
        {
            set => UpdateProperty(ref _sourceTypeEditable, value);
            get => _sourceTypeEditable;
        }
        // 源盘盘位是否可编辑
        private bool _sourceTemplateIndexEditable = true;
        public bool SourceTemplateIndexEditable
        {
            set => UpdateProperty(ref _sourceTemplateIndexEditable, value);
            get => _sourceTemplateIndexEditable;
        }
        // 源盘耗材是否可编辑
        private bool _sourceConsumableEditable = true;
        public bool SourceConsumableEditable
        {
            set => UpdateProperty(ref _sourceConsumableEditable, value);
            get => _sourceConsumableEditable;
        }
        // 源盘孔是否可编辑
        private bool _sourceHoleRangeEditable = true;
        public bool SourceHoleRangeEditable
        {
            set => UpdateProperty(ref _sourceHoleRangeEditable, value);
            get => _sourceHoleRangeEditable;
        }

        /**
         * 靶盘
         */
        // 盘位类型Index（发酵区 或 留样区）
        private int _targetTemplateTypeIndex = 0;
        public int TargetTemplateTypeIndex
        {
            set => UpdateProperty(ref _targetTemplateTypeIndex, value);
            get => _targetTemplateTypeIndex;
        }
        // 盘位可用List
        private List<int> _targetTemplateList = new List<int>();
        public List<int> TargetTemplateList
        {
            get
            {
                _targetTemplateList.Clear();
                _targetTemplateList.AddRange(new List<int> { 1, 2, 3, 4 });
                return _targetTemplateList;
            }
        }
        // 选择的盘位
        private int _targetTemplate = 0;
        public int TargetTemplate
        {
            set => UpdateProperty(ref _targetTemplate, value);
            get => _targetTemplate;
        }
        // 耗材
        private List<string> _targetConsumableList = new List<string>();
        public List<string> TargetConsumableList
        {
            get
            {
                _targetConsumableList.Clear();
                _targetConsumableList.AddRange(ParamsHelper.CommonSettingList[0].Consumables.Select(p => p.GroupName).ToList());
                return _targetConsumableList;
            }
        }
        // 靶盘耗材Index
        private string _targetConsumable = string.Empty;
        public string TargetConsumable
        {
            set => UpdateProperty(ref _targetConsumable, value);
            get => _targetConsumable;
        }
        // 靶盘孔范围
        private string _targetHoleRange = string.Empty;
        public string TargetHoleRange
        {
            set => UpdateProperty(ref _targetHoleRange, value);
            get => _targetHoleRange;
        }
        // 靶盘孔是否可编辑
        private bool _targetHoleRangeEditable = true;
        public bool TargetHoleRangeEditable
        {
            set => UpdateProperty(ref _targetHoleRangeEditable, value);
            get => _targetHoleRangeEditable;
        }
        // 每孔体积
        private decimal _volumeEach = 0.0m;
        public decimal VolumeEach
        {
            set => UpdateProperty(ref _volumeEach, value);
            get => _volumeEach;
        }
        // 每孔体积是否可编辑
        private bool _volumeEachEditable = true;
        public bool VolumeEachEditable
        {
            set => UpdateProperty(ref _volumeEachEditable, value);
            get => _volumeEachEditable;
        }
        // 靶盘是否可编辑
        private bool _targetTemplateEditable = true;
        public bool TargetTemplateEditable
        {
            set => UpdateProperty(ref _targetTemplateEditable, value);
            get => _targetTemplateEditable;
        }

        /**
         * 清洗区
         */
        // 是否需要清洗
        private bool _isNeedCleaning = true;
        public bool IsNeedCleaning
        {
            set => UpdateProperty(ref _isNeedCleaning, value);
            get => _isNeedCleaning;
        }
        // 清洗是否可编辑
        private bool _cleaningEditable = true;
        public bool CleaningEditable
        {
            set => UpdateProperty(ref _cleaningEditable, value);
            get => _cleaningEditable;
        }
        // 清洗区所有位置
        public List<string> CleaningPosList
        {
            get
            {
                var list = new List<string> { "1", "2", "3", "4", "5", "6", "无" };
                return list;
            }
        }
        // 清洗区选择的位置Index（-1为无）
        private int[] _cleaningPosIndexArray = new int[] { 0, -1, -1 };
        public int[] CleaningPosIndexArray
        {
            set => UpdateProperty(ref _cleaningPosIndexArray, value);
            get => _cleaningPosIndexArray;
        }

        ///**
        // * 吸前混合
        // */
        //// 混合体积
        //private decimal _absorbMixingVolume = 0.0m;
        //public decimal AbsorbMixingVolume
        //{
        //    set => UpdateProperty(ref _absorbMixingVolume, value);
        //    get => _absorbMixingVolume;
        //}
        //// 混合次数
        //private int _absorbMixingTimes = 0;
        //public int AbsorbMixingTimes
        //{
        //    set => UpdateProperty(ref _absorbMixingTimes, value);
        //    get => _absorbMixingTimes;
        //}
        //// 吸前混合是否可编辑
        //private bool _absorbMixingEditable = true;
        //public bool AbsorbMixingEditable
        //{
        //    set => UpdateProperty(ref _absorbMixingEditable, value);
        //    get => _absorbMixingEditable;
        //}

        ///**
        //* 喷后混合
        //*/
        //// 混合体积
        //private decimal _jetMixingVolume = 0.0m;
        //public decimal JetMixingVolume
        //{
        //    set => UpdateProperty(ref _jetMixingVolume, value);
        //    get => _jetMixingVolume;
        //}
        //// 混合次数
        //private int _jetMixingTimes = 0;
        //public int JetMixingTimes
        //{
        //    set => UpdateProperty(ref _jetMixingTimes, value);
        //    get => _jetMixingTimes;
        //}
        //// 喷后混合是否可编辑
        //private bool _jetMixingEditable = true;
        //public bool JetMixingEditable
        //{
        //    set => UpdateProperty(ref _jetMixingEditable, value);
        //    get => _jetMixingEditable;
        //}

        /**
        * 温度（发酵温控、留样温控）
        */
        // 温度
        private decimal _temp = 0;
        public decimal Temp
        {
            set => UpdateProperty(ref _temp, value);
            get => _temp;
        }
        // 温度是否可编辑
        private bool _tempEditable = true;
        public bool TempEditable
        {
            set => UpdateProperty(ref _tempEditable, value);
            get => _tempEditable;
        }
        //// 预热
        //private bool _preTemp = true;
        //public bool PreTemp
        //{
        //    set
        //    {
        //        UpdateProperty(ref _preTemp, value);
        //        // 预热勾选了，时间禁用
        //        TimeEditable = !_preTemp;
        //    }
        //    get => _preTemp;
        //}

        //// 预热是否可编辑
        //private bool _preTempEditable = true;
        //public bool PreTempEditable
        //{
        //    set => UpdateProperty(ref _preTempEditable, value);
        //    get => _preTempEditable;
        //}

        /**
         * 循环
         */
        // 循环起止位置：LoopRange[0]---如1为开始步骤1；LoopRange[1]---如2为结束步骤2
        private int[] _loopStepRange = new[] { 1, 1 };
        public int[] LoopStepRange
        {
            set => UpdateProperty(ref _loopStepRange, value);
            get => _loopStepRange;
        }
        // 循环次数
        private int _loopTimes = 1;
        public int LoopTimes
        {
            set => UpdateProperty(ref _loopTimes, value);
            get => _loopTimes;
        }
        // 循环是否可编辑
        private bool _loopEditable;
        public bool LoopEditable
        {
            set => UpdateProperty(ref _loopEditable, value);
            get => _loopEditable;
        }

        /**
         * 时间（包括搅拌时间、发酵温控时间、留样温控时间）
         */
        // 时间（分钟）
        private int _time = 0;
        public int Time
        {
            set => UpdateProperty(ref _time, value);
            get => _time;
        }
        // 时间是否可编辑
        private bool _timeEditable;
        public bool TimeEditable
        {
            set => UpdateProperty(ref _timeEditable, value);
            get => _timeEditable;
        }
        // 阻塞
        private bool _block = true;
        public bool Block
        {
            set
            {
                UpdateProperty(ref _block, value);
                if (!_block && _stepType == ObjectUtils.GetDescriptionFromEnum(EStepType.Wait))
                    WaitForOrderEditable = true;
                else
                    WaitForOrderEditable = false;
            }
            get => _block;
        }

        // 阻塞是否可编辑
        private bool _blockEditable = true;
        public bool BlockEditable
        {
            set => UpdateProperty(ref _blockEditable, value);
            get => _blockEditable;
        }
        // 等待序号List
        private List<int> _waitForOrderList = new List<int>();
        public List<int> WaitForOrderList
        {
            set => UpdateProperty(ref _waitForOrderList, value);
            get => _waitForOrderList;
        }
        // 等待序号（即从哪个步骤序号开始等待计时）
        private int _waitForOrder = 0;
        public int WaitForOrder
        {
            set => UpdateProperty(ref _waitForOrder, value);
            get => _waitForOrder;
        }
        // 等待序号是否可编辑
        private bool _waitForOrderEditable = true;
        public bool WaitForOrderEditable
        {
            set => UpdateProperty(ref _waitForOrderEditable, value);
            get => _waitForOrderEditable;
        }


        /**
         * 速度等级（搅拌等）
         */
        // 速度等级列表
        private List<int> _speedLevelList = new List<int>();
        public List<int> SpeedLevelList
        {
            get
            {
                _speedLevelList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                return _speedLevelList;
            }
        }
        // 选择的速度等级
        private int _speedLevel = 1;
        public int SpeedLevel
        {
            set => UpdateProperty(ref _speedLevel, value);
            get => _speedLevel;
        }
        // 速度等级是否可编辑
        private bool _speedLevelEditable = true;
        public bool SpeedLevelEditable
        {
            set => UpdateProperty(ref _speedLevelEditable, value);
            get => _speedLevelEditable;
        }



        private void UpdateProperty<T>(ref T properValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            properValue = newValue;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
