using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAERun
{
    public class MotorPosition
    {
        private int xPos;
        private int yPos;
        private int zPos;
        private int pPos;
        private int wPos;
        private int qPos;
        private WorkPlate workPlate;
        public MotorPosition(WorkPlate wrkPlate)
        {
            workPlate = wrkPlate;
        }

        public int XPos
        {
            get
            {
                return xPos;
            }

            set
            {
                xPos = value;
            }
        }

        public int YPos
        {
            get
            {
                return yPos;
            }

            set
            {
                yPos = value;
            }
        }

        public int ZPos
        {
            get
            {
                return zPos;
            }

            set
            {
                zPos = value;
            }
        }

        public int PPos
        {
            get
            {
                return pPos;
            }

            set
            {
                pPos = value;
            }
        }

        public int WPos
        {
            get
            {
                return wPos;
            }

            set
            {
                wPos = value;
            }
        }

        public int QPos
        {
            get
            {
                return qPos;
            }

            set
            {
                qPos = value;
            }
        }

        public decimal XPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Xcale : 1.0m;
                return Math.Round(XPos / scale-workPlate.WorkPlateX0,2);
            }
        }

        public decimal YPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Ycale : 1.0m;
                return Math.Round(YPos / scale - workPlate.WorkPlateY0, 2);
            }
        }

        public decimal ZPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Zcale : 1.0m;
                return Math.Round(ZPos / scale-workPlate.WorkPlateZ0, 2);
            }
        }

        public decimal PPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Pcale : 1.0m;
                return Math.Round(PPos / scale,2);
            }
        }

        public decimal WPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Wcale : 1.0m;
                return Math.Round(WPos / scale-workPlate.WorkPlateW0, 2);
            }
        }

        public decimal QPosf
        {
            get
            {
                decimal scale = (workPlate != null) ? workPlate.Qcale : 1.0m;
                return Math.Round(QPos / scale,2);
            }
        }


    }
}
