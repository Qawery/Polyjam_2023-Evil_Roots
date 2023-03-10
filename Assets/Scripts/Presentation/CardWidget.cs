using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Polyjam2023
{
    public class CardWidget : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI cardName;
        [SerializeField] private Image cardImage;
        [SerializeField] private Image quantityImage;
        [SerializeField] private TMPro.TextMeshProUGUI quantityText;
        [SerializeField] private TMPro.TextMeshProUGUI cardDescription;
        [SerializeField] private Button button;
        
        [SerializeField] private GameObject unitStatsSection;
        [SerializeField] private TMPro.TextMeshProUGUI attackValue;
        [SerializeField] private TMPro.TextMeshProUGUI initiativeValue;
        [SerializeField] private TMPro.TextMeshProUGUI healthValue;
        private RectTransform rectTransform;

        public static event System.Action<CardWidget> OnCardWidgetClicked;
        
        private ICardLocation CardLocation { get; set; }
        public string CardName => cardName.text;
        public RectTransform RectTransform => rectTransform;
        
        protected virtual void Awake()
        {
            Assert.IsNotNull(cardName, $"Missing {nameof(cardName)} on {gameObject.name}.");
            Assert.IsNotNull(cardImage, $"Missing {nameof(cardImage)} on {gameObject.name}.");
            Assert.IsNotNull(quantityImage, $"Missing {nameof(quantityImage)} on {gameObject.name}.");
            Assert.IsNotNull(quantityText, $"Missing {nameof(quantityText)} on {gameObject.name}.");
            Assert.IsNotNull(cardDescription, $"Missing {nameof(cardDescription)} on {gameObject.name}.");
            Assert.IsNotNull(button, $"Missing {nameof(button)} on {gameObject.name}.");
            button.onClick.AddListener(() => { OnCardWidgetClicked?.Invoke(this); } );
            
            Assert.IsNotNull(unitStatsSection, $"Missing {nameof(unitStatsSection)} on {gameObject.name}.");
            Assert.IsNotNull(attackValue, $"Missing {nameof(attackValue)} on {gameObject.name}.");
            Assert.IsNotNull(initiativeValue, $"Missing {nameof(initiativeValue)} on {gameObject.name}.");
            Assert.IsNotNull(healthValue, $"Missing {nameof(healthValue)} on {gameObject.name}.");
            rectTransform = gameObject.GetComponent<RectTransform>();
            Assert.IsNotNull(rectTransform, $"Missing {nameof(rectTransform)} on {gameObject.name}.");
        }

        protected virtual void OnDestroy()
        {
            cardName = null;
            cardImage = null;
            quantityImage = null;
            quantityText = null;
            cardDescription = null;
            button.onClick.RemoveAllListeners();
            button = null;
            
            unitStatsSection = null;
            attackValue = null;
            initiativeValue = null;
            healthValue = null;
            rectTransform = null;
        }

        public virtual void SetPresentationData(ICardLocation cardLocation, CardTemplate cardTemplate, int quantity)
        {
            Assert.IsNotNull(cardLocation, $"Trying to assign null {nameof(cardLocation)} to {nameof(CardWidget)}.");
            CardLocation = cardLocation;
            
            cardName.text = cardTemplate.CardName;
            cardImage.sprite = cardTemplate.Image;
            Assert.IsTrue(quantity > 0, "Trying to write card quantity not above zero.");
            if (quantity > 1)
            {
                quantityText.text = $"x{quantity.ToString()}";
                quantityImage.gameObject.SetActive(true);
            }
            else
            {
                quantityImage.gameObject.SetActive(false);
            }
            cardDescription.text = cardTemplate.EffectDescription;
            
            if (cardTemplate is UnitCardTemplate)
            {
                var unitCardTemplate = cardTemplate as UnitCardTemplate;
                unitStatsSection.SetActive(true);
                attackValue.text = unitCardTemplate.Attack.ToString();
                initiativeValue.text = unitCardTemplate.Initiative.ToString();
                healthValue.text = unitCardTemplate.Health.ToString();
            }
            else
            {
                unitStatsSection.SetActive(false);
            }
        }
    }
}
