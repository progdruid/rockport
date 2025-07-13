using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{
    public class FirefliesSpawner : EntityComponent
    {
        //fields//////////////////////////////////////////////////////////////////////////////////////////////////////
        [SerializeField] private new ParticleSystem particleSystem;
        
        private float _ratePerUnit = 0f;
        private const float UnitArea = 100f;

        //initialisation////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void Wake()
        {
            Assert.IsNotNull(particleSystem);
            
            var shapeModule = particleSystem.shape;
            var emissionModule = particleSystem.emission;
            _ratePerUnit = emissionModule.rateOverTime.constant /
                           (shapeModule.scale.x * shapeModule.scale.y / UnitArea);
        }

        public override void Initialise()
        {
            var mapWorldSize = Space.ConvertMapToWorld(Space.MapSize);
            var shapeModule = particleSystem.shape;
            shapeModule.scale = new Vector3(mapWorldSize.x, mapWorldSize.y, shapeModule.scale.z);
            shapeModule.position = new Vector3(mapWorldSize.x / 2f, mapWorldSize.y / 2f, shapeModule.position.z);
            var rate = _ratePerUnit * (mapWorldSize.x * mapWorldSize.y / UnitArea);
            var emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = rate;
            particleSystem.Clear();
            particleSystem.Play();
        }
        
        public override void Activate() { }

        
        //public interface////////////////////////////////////////////////////////////////////////////////////////////////
        public override string JsonName => "fireflies-spawner";

        public override IEnumerator<PropertyHandle> GetProperties() { yield break; }
        
        public override void Replicate(JSONNode data) { }
        public override JSONNode ExtractData() => new JSONObject();
    }
}