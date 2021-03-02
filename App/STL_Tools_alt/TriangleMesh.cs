﻿/**
  ******************************************************************************
  * @file    TriangleMesh.cs
  * @author  Ali Batuhan KINDAN
  * @version V1.0.0
  * @date    03.07.2018
  * @brief   
  ******************************************************************************
  */

namespace STL_Tools_alt
{
    public class TriangleMesh
    {
        public Vector3 normal1;
        public Vector3 normal2;
        public Vector3 normal3;
        public Vector3 vert1;
        public Vector3 vert2;
        public Vector3 vert3;
        public RColor col1;
        public RColor col2;
        public RColor col3;

        /**
        * @brief  Class instance constructor
        * @param  none
        * @retval none
        */
        public TriangleMesh()
        {
            normal1 = new Vector3();
            normal2 = new Vector3();
            normal3 = new Vector3();
            vert1 = new Vector3();
            vert2 = new Vector3();
            vert3 = new Vector3();
            col1 = new RColor();
            col2 = new RColor();
            col3 = new RColor();
        }
    }

}
