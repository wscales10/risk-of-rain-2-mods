using UnityEngine;

namespace PactOfPunishment
{
    public abstract class PointVisualizer<TComponent> : MonoBehaviour
    {
        private GameObject sphere;

        protected TComponent Component { get; private set; }

        public void Awake()
        {
            this.Component = this.GetComponent<TComponent>();
            this.sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            this.sphere.transform.localScale = Vector2.one * 0.5f;
        }

        protected abstract Vector3 GetPosition();

        public void Update()
        {
            this.sphere.transform.position = this.GetPosition();
        }

        public void OnDestroy()
        {
            Destroy(this.sphere);
        }
    }
}
