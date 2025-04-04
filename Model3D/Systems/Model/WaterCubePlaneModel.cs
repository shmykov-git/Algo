﻿using System;
using System.Collections.Generic;
using Model;
using Model3D.Tools;

namespace Model3D.Systems.Model
{
    public class WaterCubeModel
    {
        public Func<int, IAnimatorParticleItem[]> GetInitItemsFn;
        public Func<int, IAnimatorParticleItem[]> GetStepItemsFn;
        public Func<IAnimatorParticleItem, bool> GetParticleFilterFn;
        public Action<Shape> ModifyParticleFn;

        public List<WaterCubePlaneModel> PlaneModels;
        public bool RunCalculations = true;
        public bool DebugColliders;
        public bool DebugCollidersNoVisible;
        public bool DebugCollidersSkipCube;
        public bool DebugCollidersSkipShift;
        public bool DebugCollidersAsLines;
        public double DebugCollidersAsLinesThikness = 1;
        public bool DebugNetPlanes;
    }

    public class WaterCubePlaneModel
    {
        public Shape VisibleShape;
        public Shape ColliderShape;
        public double ColliderShift = 0;
        public bool DebugColliderSkip;
        public bool SkipVisible;
        public bool Debug;
    }
}