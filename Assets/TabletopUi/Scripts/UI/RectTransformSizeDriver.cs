#pragma warning disable 0649
using System.Collections.Generic;
using System.Linq;
using Assets.Core;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.TabletopUi.Scripts
{
    /// <summary>
    /// displays a summary of aspects; used for the workspace display, and in the recipe book
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
	public class RectTransformSizeDriver : UIBehaviour {

        // This is pushed to the Aspect Frame 
        // There it is used in the click>Notifier call to tell the notifier where to place the details window
		[SerializeField] RectTransform.Axis m_axis;
		[SerializeField] RectTransform m_sizeTarget;
		[SerializeField] float m_sizeBase = 370f;

		public RectTransform.Axis axis { 
			get { return m_axis; } 
			set { m_axis = value; 
				UpdateTracker(); 
				UpdateTargetSize(); } 
		}
		
		public RectTransform sizeTarget { 
			get { return m_sizeTarget; } 
			set { m_sizeTarget = value; 
				UpdateTracker(); 
				UpdateTargetSize(); } 
		}
		
		public float sizeBase { 
			get { return m_sizeBase; } 
			set { m_sizeBase = value; 
				UpdateTargetSize(); } 
		}

		protected DrivenRectTransformTracker m_Tracker;

		new void OnEnable() {
			UpdateTracker();
			UpdateTargetSize();
		}

		void UpdateTracker() {
			m_Tracker.Clear();

			if (m_sizeTarget == null)
				return;

			if (m_axis == RectTransform.Axis.Horizontal)
				m_Tracker.Add(this, m_sizeTarget, DrivenTransformProperties.SizeDeltaX);
			else 
				m_Tracker.Add(this, m_sizeTarget, DrivenTransformProperties.SizeDeltaY);
		}

		new void OnDisable() {
			m_Tracker.Clear();
		}

		protected override void OnRectTransformDimensionsChange() {
			base.OnRectTransformDimensionsChange();
			UpdateTargetSize();
		}

		void UpdateTargetSize() {
			float axisSize;

			if (m_axis == RectTransform.Axis.Horizontal)
				axisSize = ((RectTransform)transform).sizeDelta.x;
			else
				axisSize = ((RectTransform)transform).sizeDelta.y;
			
			m_sizeTarget.SetSizeWithCurrentAnchors(m_axis, m_sizeBase + axisSize);
		}

    }




}
