using System;
using System.Collections.Generic;
using Model;
using Model3D.Tools;

namespace Model3D.Systems.Model
{
    public class WaterCubeModel
    {
        public Func<int, IAnimatorParticleItem[]> GetInitItemsFn;
        public Func<int, IAnimatorParticleItem[]> GetStepItemsFn;

        public List<WaterCubePlaneModel> PlaneModels;
    }

    public class WaterCubePlaneModel
    {
        public Shape VisibleShape;
        public Shape LogicShape;
        public WaterCubeColliderStrategy ColliderStrategy = WaterCubeColliderStrategy.MovePlanes;
        public bool ReversePlaneNormals;
        public bool SkipLogic;
        public bool SkipVisible;
    }

    public enum WaterCubeColliderStrategy
    {
        AddBorder,
        MovePlanes
    }
}