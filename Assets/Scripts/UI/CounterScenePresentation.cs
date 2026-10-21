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
        [SerializeField] private BurgerAssemblyPresentation burger;
        [SerializeField] private RectTransform cookingNamePanel;
        [SerializeField] private Collider dishCollider;
        [SerializeField] private UnityEngine.UI.Button[] sheets;
        [SerializeField] private float boardYawBias = 12f;
        private Quaternion returnRotation;
        private float returnFov;
        private bool followWasEnabled, leftWasEnabled, rightWasEnabled, focused, initialized, wasBoard, wasCooking;
        private PrototypeOrder presentedOrder;
        private Vector3 defaultColliderSize, defaultColliderCenter;
        private UnityEngine.UI.Text cookingNameLabel;
        private Vector2 defaultNamePosition, defaultNameSize, defaultNameTextSize;
        private Vector3 boardSurfaceNormal;

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
            leftWasEnabled=boardPaperLeft.enabled;rightWasEnabled=boardPaperRight.enabled;initialized=true;
            // FBX re-exports can change mesh axes without changing the physical paper plane.
            var paperMesh=boardPaperLeft.GetComponent<MeshFilter>();
            if(paperMesh!=null&&paperMesh.sharedMesh!=null)
            {
                Vector3 size=paperMesh.sharedMesh.bounds.size;
                Vector3 axis=size.x<=size.y&&size.x<=size.z?Vector3.right:size.y<=size.z?Vector3.up:Vector3.forward;
                boardSurfaceNormal=boardPaperLeft.transform.TransformDirection(axis).normalized;
            }
            else boardSurfaceNormal=-boardPaperLeft.transform.forward;
            if(Vector3.Dot(boardSurfaceNormal,sceneCamera.transform.position-boardPaperLeft.bounds.center)<0)
                boardSurfaceNormal=-boardSurfaceNormal;
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
            if(!initialized)return;
            bool cooking=cookingScreen.activeInHierarchy;
            bool result=resultScreen.activeInHierarchy;
            bool board=workDayScreen.activeInHierarchy;
            bool showSheets=board;
            boardCanvas.gameObject.SetActive(showSheets);
            if(board&&!wasBoard)
            {
                foreach(var sheet in sheets)
                    if(sheet.gameObject.activeInHierarchy&&sheet.interactable)
                    { var events=UnityEngine.EventSystems.EventSystem.current;if(events!=null)events.SetSelectedGameObject(sheet.gameObject);break; }
            }
            wasBoard=board;
            boardPaperLeft.enabled=showSheets?false:leftWasEnabled;
            boardPaperRight.enabled=showSheets?false:rightWasEnabled;
            // Anchors follow the actual environment, not a viewport coordinate.
            boardCanvas.rotation=Quaternion.LookRotation(-boardSurfaceNormal,Vector3.up);
            boardCanvas.position=(boardPaperLeft.bounds.center+boardPaperRight.bounds.center)*.5f + Vector3.up*.04f-boardCanvas.forward*.055f;
            int count=controller.OrderSession.BoardCapacity;
            int columns=Mathf.Min(3,count);
            for(int i=0;i<count;i++)
            {
                bool compact=count>3;int row=i/columns;int rowCount=Mathf.Min(columns,count-row*columns);
                var rt=sheets[i].GetComponent<RectTransform>();
                rt.sizeDelta=new Vector2(compact?285:350,compact?315:490);
                rt.anchoredPosition3D=new Vector3(((i%columns)-(rowCount-1)*.5f)*(compact?310:385),compact?175-row*350:0,0);
                var name=sheets[i].transform.GetChild(0).GetComponent<UnityEngine.UI.Text>();
                if(name!=null){name.rectTransform.sizeDelta=new Vector2(compact?265:330,compact?120:135);name.rectTransform.anchoredPosition=new Vector2(0,compact?95:155);name.fontSize=compact?32:38;}
                var illustration=sheets[i].transform.Find("UIA2_Plate") as RectTransform;
                illustration.sizeDelta=new Vector2(compact?120:230,compact?90:180);illustration.anchoredPosition=Vector2.zero;
                var mastery=sheets[i].transform.Find("UIA3_CardMasteryText").GetComponent<UnityEngine.UI.Text>();
                mastery.rectTransform.sizeDelta=new Vector2(compact?265:330,65);mastery.rectTransform.anchoredPosition=new Vector2(0,compact?-80:-135);
                var track=sheets[i].transform.Find("UIA3_CardMastery") as RectTransform;
                track.sizeDelta=new Vector2(compact?255:310,22);track.anchoredPosition=new Vector2(0,compact?-132:-205);
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
            sandwich.SetActive(dish!=null&&dish.DishId==1);
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
            if(board&&cursorFollow!=null&&cursorFollow.enabled)
                sceneCamera.transform.localRotation=cursorFollow.CursorRotation*Quaternion.Euler(0,boardYawBias,0);
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
            if(boardPaperLeft!=null)boardPaperLeft.enabled=leftWasEnabled;
            if(boardPaperRight!=null)boardPaperRight.enabled=rightWasEnabled;
            if(boardCanvas!=null)boardCanvas.gameObject.SetActive(false);
            if(dishRoot!=null)dishRoot.SetActive(false);
            SetBurgerNameLayout(false);
        }
    }
}
