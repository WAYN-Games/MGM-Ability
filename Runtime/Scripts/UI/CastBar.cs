
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.UIElements;

namespace WaynGroup.Mgm.Ability.UI
{
    class CastBar : EntityOwnedUpdatableVisualElement
    {
        /// <summary>
        /// Represent the precision format to use for the cast bar text.
        /// i.e. 0.01 will display a 2 decimal precison.
        /// Default is 0.1
        /// </summary>
        public float precisionAttr { get; set; }
        private VisualElement root;
        private ProgressBar pb;


        public new class UxmlFactory : UxmlFactory<CastBar, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription _precision = new UxmlFloatAttributeDescription { name = "precision-attr", defaultValue = 0.1F };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                CastBar cb = ve as CastBar;
                cb.precisionAttr = _precision.GetValueFromBag(bag, cc);
            }
        }

        public CastBar()
        {
            VisualTreeAsset visualTree = Resources.Load<VisualTreeAsset>("CastBar");
            visualTree.CloneTree(this);
            precisionAttr = 0.1f;
            pb = this.Q<ProgressBar>();
            pb.SetOrientation(FlexDirection.Row);
            SetProgresss(0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percent">value between 0 and 100</param>
        public void SetProgresss(float percent)
        {
            if (percent <= 0 && style.visibility != Visibility.Hidden)
            {
                style.visibility = Visibility.Hidden;
                return;
            }
            else if (percent > 0 && style.visibility != Visibility.Visible)
            {
                style.visibility = Visibility.Visible;
            }
            pb.Value = percent;
            pb.Title = "";
        }


        public override void Update()
        {
            CurrentlyCasting cc = _entityManager.GetComponentData<CurrentlyCasting>(_owner);
            if (cc.IsCasting)
            {
                var AbilityTimingsCache = _entityManager.CreateEntityQuery(new Unity.Entities.ComponentType[] { Unity.Entities.ComponentType.ReadOnly<AbilityTimingsCache>() }).GetSingleton<AbilityTimingsCache>();
                ref var cache = ref AbilityTimingsCache.Cache.Value;

                SetProgresss(cc.castTime / cache.GetValuesForKey(cc.abilityGuid)[0].Cast * 100);
                pb.Title = $"{math.round(cc.castTime / precisionAttr) * precisionAttr} s";
            }
            else
            {
                SetProgresss(-1f);
            }
        }
    }



}
