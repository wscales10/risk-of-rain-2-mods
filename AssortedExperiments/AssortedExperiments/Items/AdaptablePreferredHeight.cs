using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AssortedExperiments.Items
{
    [RequireComponent(typeof(LayoutElement))]
    public class AdaptablePreferredHeight : UIBehaviour, ILayoutElement
    {
        public float MaxHeight = 500;
        public RectTransform ContentToGrowWith;

        public int LayoutPriority = 10;

        LayoutElement m_LayoutElement;
        float m_Preferredheight;

        public float minWidth => this.m_LayoutElement.minWidth;
        public float preferredWidth => this.m_LayoutElement.preferredWidth;
        public float flexibleWidth => this.m_LayoutElement.flexibleWidth;
        public float minHeight => this.m_LayoutElement.minHeight;
        public float preferredHeight => this.m_Preferredheight;
        public float flexibleHeight => this.m_LayoutElement.flexibleHeight;
        public int layoutPriority => this.LayoutPriority;

        public void CalculateLayoutInputHorizontal()
        {
            if (this.m_LayoutElement == null)
            {
                this.m_LayoutElement = this.GetComponent<LayoutElement>();
            }
        }

        public void CalculateLayoutInputVertical()
        {
            if (this.m_LayoutElement == null)
            {
                this.m_LayoutElement = this.GetComponent<LayoutElement>();
            }

            float contentHeight = this.ContentToGrowWith.sizeDelta.y;

            if (contentHeight < this.MaxHeight)
            {
                this.m_Preferredheight = contentHeight;
            }
            else
            {
                this.m_Preferredheight = this.MaxHeight;
            }
        }

    }
}