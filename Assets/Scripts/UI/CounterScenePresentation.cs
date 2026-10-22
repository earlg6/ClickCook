using ClickCook.Runtime;
using UnityEngine;

namespace ClickCook.UI
{
    /// <summary>Scene-only presentation of the existing order session. No order, reward or damage logic.</summary>
    public sealed class CounterScenePresentation : MonoBehaviour
    {
        [SerializeField] private ClickCookPrototypeController controller;
        [SerializeField] private Camera sceneCamera;
        [SerializeField] private CameraCursorFollow cursorFollow;
        [SerializeField] private GameObject workDayScreen, cookingScreen, resultScreen;
        [SerializeField] private RectTransform boardCanvas;
        [SerializeField] private Renderer boardPaperLeft, boardPaperRight, plate;
        [SerializeField] private GameObject dishRoot, sandwich, mugCake, genericDish;
        [SerializeField] private SandwichAssemblyPresentation sandwichAssembly;
        [SerializeField] private BurgerAssemblyPresentation burger;
        [SerializeField] private RectTransform cookingNamePanel;
        [SerializeField] private Collider dishCollider;
        [SerializeField] private UnityEngine.UI.Button[] sheets;
        [SerializeField, Min(.01f)] private float cardEntranceSeconds = .28f;
        [SerializeField, Min(0f)] private float cardStaggerSeconds = .025f;
        private Quaternion returnRotation;
        private float returnFov;
        private bool followWasEnabled, focused, initialized, wasBoard, wasCooking;
        private PrototypeOrder presentedOrder;
        private Vector3 defaultColliderSize, defaultColliderCenter;
        private UnityEngine.UI.Text cookingNameLabel;
        private Vector2 defaultNamePosition, defaultNameSize, defaultNameTextSize;
        private CanvasGroup cardGroup;
        private float entranceTime;

        public bool OwnsOrderCardLayout => boardCanvas != null;
        public bool OrderCardsReady { get; private set; }

        private void Start()
        {
            if (controller == null || sceneCamera == null || boardCanvas == null || plate == null ||
                boardPaperLeft == null || boardPaperRight == null || dishRoot == null || dishCollider == null ||
                sandwich == null || mugCake == null || genericDish == null || workDayScreen == null ||
                cookingScreen == null || resultScreen == null || sheets == null || sheets.Length != 6)
            { Debug.LogError("Counter presentation is missing scene bindings.",this); enabled=false; return; }
            foreach (var sheet in sheets)
                if (!HasSheetBindings(sheet))
                { Debug.LogError("Counter presentation has an incomplete order sheet.", this); enabled = false; return; }
            cardGroup=boardCanvas.GetComponent<CanvasGroup>();
            if(cardGroup==null||boardCanvas.GetComponent<Canvas>().renderMode==RenderMode.WorldSpace)
            { Debug.LogError("Front order selection requires its configured overlay CanvasGroup.",this); enabled=false; return; }
            initialized=true;
            if (dishCollider is BoxCollider box)
            { defaultColliderSize=box.size; defaultColliderCenter=box.center; }
            if(cookingNamePanel!=null)
            {
                cookingNameLabel=cookingNamePanel.GetComponentInChildren<UnityEngine.UI.Text>(true);
                defaultNamePosition=cookingNamePanel.anchoredPosition;defaultNameSize=cookingNamePanel.sizeDelta;
                if(cookingNameLabel!=null)defaultNameTextSize=cookingNameLabel.rectTransform.sizeDelta;
            }
        }

        private void LateUpdate()
        {
            // A script-domain reload during Play Mode can run this component one frame before
            // the controller recreates its transient session.
            if(!initialized||controller==null||controller.OrderSession==null)return;
            if(controller.OrderSession.IsPaused)return;
            bool cooking=cookingScreen.activeInHierarchy;
            bool result=resultScreen.activeInHierarchy;
            bool board=workDayScreen.activeInHierarchy;
            bool showSheets=board&&controller.OrderSession.Orders.Count>0;
            boardCanvas.gameObject.SetActive(showSheets);
            if(showSheets&&!wasBoard)
            {
                entranceTime=0f;
                foreach(var sheet in sheets)
                    if(sheet.gameObject.activeInHierarchy&&sheet.interactable)
                    { var events=UnityEngine.EventSystems.EventSystem.current;if(events!=null)events.SetSelectedGameObject(sheet.gameObject);break; }
            }
            else if(showSheets)entranceTime+=Time.unscaledDeltaTime;
            wasBoard=showSheets;
            int count=controller.OrderSession.BoardCapacity;
            int columns=Mathf.Min(3,count);
            OrderCardsReady=!showSheets||entranceTime>=cardEntranceSeconds+Mathf.Max(0,count-1)*cardStaggerSeconds;
            // Mouse hits wait until the moving cards settle. Keyboard Submit remains immediately available.
            cardGroup.blocksRaycasts=showSheets&&OrderCardsReady;
            for(int i=0;i<count;i++)
            {
                bool compact=count>3;int row=i/columns;int rowCount=Mathf.Min(columns,count-row*columns);
                var rt=sheets[i].GetComponent<RectTransform>();
                float width=compact?360:count==2?420:360;
                rt.sizeDelta=new Vector2(width,compact?300:540);
                var target=new Vector2(((i%columns)-(rowCount-1)*.5f)*(count==2?500:420),compact?150-row*330:-15);
                float t=Mathf.Clamp01((entranceTime-i*cardStaggerSeconds)/Mathf.Max(.01f,cardEntranceSeconds));
                float eased=1f-Mathf.Pow(1f-t,3f);
                rt.anchoredPosition3D=Vector2.Lerp(new Vector2(boardCanvas.rect.width*.5f+width+i*65,target.y-45),target,eased);
                rt.localScale=Vector3.one*Mathf.Lerp(.2f,1f,eased);
                rt.localRotation=Quaternion.identity;
                var name=sheets[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
                if(name!=null){name.rectTransform.sizeDelta=new Vector2(width-30,compact?80:135);name.rectTransform.anchoredPosition=new Vector2(0,compact?105:175);name.fontSize=compact?28:34;}
                var illustration=sheets[i].transform.Find("UIA2_Plate") as RectTransform;
                illustration.sizeDelta=new Vector2(compact?150:260,compact?105:200);illustration.anchoredPosition=new Vector2(0,compact?0:10);
                var mastery=sheets[i].transform.Find("UIA3_CardMasteryText").GetComponent<UnityEngine.UI.Text>();
                mastery.rectTransform.sizeDelta=new Vector2(width-30,compact?45:65);mastery.rectTransform.anchoredPosition=new Vector2(0,compact?-78:-135);
                var track=sheets[i].transform.Find("UIA3_CardMastery") as RectTransform;
                track.sizeDelta=new Vector2(width-50,22);track.anchoredPosition=new Vector2(0,compact?-125:-210);
                var fill=track.Find("Fill") as RectTransform;fill.sizeDelta=track.sizeDelta-new Vector2(8,8);
            }
            var dish=cooking?controller.OrderSession.ActiveOrder?.Dish:result?controller.OrderSession.LastResult?.Dish:null;
            dishRoot.SetActive(dish!=null);
            dishCollider.enabled=cooking&&dish!=null;
            var pointer = dishRoot.GetComponent<CounterDishPointer>();
            if (pointer != null)
            {
                pointer.interactable = cooking && dish != null;
                if (cooking && !wasCooking)
                {
                    var events = UnityEngine.EventSystems.EventSystem.current;
                    if (events != null) events.SetSelectedGameObject(dishRoot);
                }
            }
            wasCooking = cooking;
            var center=plate.bounds.center;
            dishRoot.transform.position=new Vector3(center.x,plate.bounds.max.y+.008f,center.z);
            bool showSandwich=dish!=null&&dish.DishId==1&&sandwichAssembly!=null;
            sandwich.SetActive(dish!=null&&dish.DishId==1&&!showSandwich);
            mugCake.SetActive(dish!=null&&dish.DishId==2);
            bool showBurger=dish!=null&&dish.DishId==6&&burger!=null;
            SetBurgerNameLayout(showBurger);
            genericDish.SetActive(dish!=null&&dish.DishId!=1&&dish.DishId!=2&&!showBurger);
            if (cooking) presentedOrder=controller.OrderSession.ActiveOrder;
            if (burger!=null)
            {
                burger.gameObject.SetActive(showBurger);
                if(showBurger)
                {
                    object identity=presentedOrder!=null&&presentedOrder.Dish==dish
                        ? presentedOrder : (object)controller.OrderSession.LastResult;
                    float progress=result?1f:1f-(float)controller.OrderSession.ActiveOrder.RemainingClicks/dish.ClickRequirement;
                    burger.Present(identity,progress);
                }
            }
            if (sandwichAssembly!=null)
            {
                sandwichAssembly.gameObject.SetActive(showSandwich);
                if(showSandwich)
                {
                    object identity=presentedOrder!=null&&presentedOrder.Dish==dish
                        ? presentedOrder : (object)controller.OrderSession.LastResult;
                    float progress=result?1f:1f-(float)controller.OrderSession.ActiveOrder.RemainingClicks/dish.ClickRequirement;
                    sandwichAssembly.Present(identity,progress);
                }
            }
            if (dishCollider is BoxCollider currentBox)
            {
                currentBox.size=showBurger?new Vector3(.44f,.40f,.36f):defaultColliderSize;
                currentBox.center=showBurger?new Vector3(0,.20f,0):defaultColliderCenter;
            }
            if(!cooking&&!result) presentedOrder=null;
            bool focus=cooking||result;
            if(focus&&!focused)
            {
                returnRotation=sceneCamera.transform.rotation;returnFov=sceneCamera.fieldOfView;
                followWasEnabled=cursorFollow!=null&&cursorFollow.enabled;
                if(cursorFollow!=null)cursorFollow.enabled=false;
            }
            if(focus)
            {
                sceneCamera.transform.LookAt(dishRoot.transform.position+new Vector3(result ? .32f : 0f,showBurger&&cooking?.08f:.10f,0));
                sceneCamera.fieldOfView=40f;
            }
            else if(focused)RestoreCamera();
            focused=focus;
        }
        private static bool HasSheetBindings(UnityEngine.UI.Button sheet)
        {
            if (sheet == null || sheet.transform.childCount == 0 ||
                sheet.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>() == null) return false;
            var mastery = sheet.transform.Find("UIA3_CardMasteryText");
            var track = sheet.transform.Find("UIA3_CardMastery");
            return sheet.transform.Find("UIA2_Plate") is RectTransform && mastery != null &&
                mastery.GetComponent<UnityEngine.UI.Text>() != null && track is RectTransform &&
                track.Find("Fill") is RectTransform;
        }
        private void SetBurgerNameLayout(bool assembling)
        {
            if(cookingNamePanel==null)return;
            cookingNamePanel.anchoredPosition=assembling?defaultNamePosition+new Vector2(-650,0):defaultNamePosition;
            cookingNamePanel.sizeDelta=assembling?new Vector2(440,150):defaultNameSize;
            if(cookingNameLabel!=null)cookingNameLabel.rectTransform.sizeDelta=assembling?new Vector2(390,130):defaultNameTextSize;
        }
        private void RestoreCamera()
        {
            if(sceneCamera!=null){sceneCamera.transform.rotation=returnRotation;sceneCamera.fieldOfView=returnFov;}
            if(cursorFollow!=null)cursorFollow.enabled=followWasEnabled;
        }
        private void OnDisable()
        {
            if(!initialized)return;
            if(focused)RestoreCamera();focused=false;
            if(boardCanvas!=null)boardCanvas.gameObject.SetActive(false);
            wasBoard=false;OrderCardsReady=false;
            if(dishRoot!=null)dishRoot.SetActive(false);
            SetBurgerNameLayout(false);
        }
    }
}
