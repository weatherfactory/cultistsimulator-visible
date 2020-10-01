using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure {
    //places, arranges and displays things on the table
    public class Choreographer:ISettingSubscriber {

        private TabletopTokenContainer _tabletop;
        private SituationBuilder _situationBuilder;
        private Rect tableRect;

        const float checkPointPerArcLength = 100f;

        const float pointGridSize = 100f;
        private float gridSnapSize = 0.0f;
        const int maxGridIterations = 5;

        const float radiusBase = 50f;
        const float radiusIncrement = 50f;
        const float radiusMaxSize = 250f;

        private ChoreographerDebugView _currentDebug;

        public Choreographer(TabletopTokenContainer tabletop, SituationBuilder situationBuilder, Transform tableLevelTransform, Transform WindowLevelTransform) {
            _tabletop = tabletop;
            _situationBuilder = situationBuilder;

            tableRect = tabletop.GetRect();

            var snapGridSetting = Registry.Get<ICompendium>().GetEntityById<Setting>(NoonConstants.GRIDSNAPSIZE);
            if (snapGridSetting != null)
            {
                snapGridSetting.AddSubscriber(this);
                WhenSettingUpdated(snapGridSetting.CurrentValue);
            }
            else
                NoonUtility.Log("Missing setting entity: " + NoonConstants.GRIDSNAPSIZE);
        }

        

        #region -- PUBLIC POSITIONING METHODS ----------------------------

        public void ArrangeTokenOnTable(SituationToken token, Context context) {
            token.RectTransform.anchoredPosition = GetFreePosWithDebug(token, Vector2.zero);

            _tabletop.DisplaySituationTokenOnTable(token, context);
        }

        // Elements are we placed differently than tokens

        public void ArrangeTokenOnTable(ElementStackToken stack, Context context) {
            ArrangeTokenOnTable(stack, context, stack.lastTablePos, false);
        }

        public void ArrangeTokenOnTable(ElementStackToken stack, Context context, Vector2? pos = null, bool pushOthers = false, bool stackBothSides = true)
		{
            _tabletop.GetElementStacksManager().AcceptStack(stack, context);  // this does parenting. Needs to happen before we position

            if (pushOthers && pos != null)
			{ 
                pos = GetPosClampedToTable(pos.Value);
            }
            else
			{
                pos = GetFreePosWithDebug(stack, pos != null ? pos.Value : Vector2.zero, -1, stackBothSides);
            }

            stack.RectTransform.anchoredPosition = pos.Value;
            stack.lastTablePos = pos.Value;
            stack.transform.localRotation = Quaternion.identity;
     
            stack.DisplayAtTableLevel();
            stack.FlipToFaceUp(true);
			stack.SnapToGrid();

            if (pushOthers)
                MoveAllTokensOverlappingWith(stack);
        }

        #endregion

        #region -- POSITIONING HELP METHODS ----------------------------

        public void MoveAllTokensOverlappingWith(DraggableToken pushingToken)
		{
			if (pushingToken.NoPush)
			{
				return;
			}

            var targetRect = GetCenterPosRect(pushingToken.RectTransform);
			// Reduce the target Rect size to be less finnicky
			targetRect.size = targetRect.size * 0.6f;

			Rect pushedRect;

            foreach (var token in _tabletop.GetTokens()) {
                if (CanTokenBeIgnored(token, pushingToken))
                    continue;

				pushedRect = GetCenterPosRect(token.RectTransform);

				if (!pushedRect.Overlaps(targetRect))
                    continue;

                AnimateTokenTo(token,
                    duration: 0.2f,
                    startPos: token.RectTransform.anchoredPosition3D,
                    endPos: GetFreePosWithDebug(token, token.RectTransform.anchoredPosition));
            }
        }

        // Note: If we're using the world pos while an object is dragged we get the wrong pos, since we project from cam through the raised pos
        public Vector2 GetTablePosForWorldPos(Vector3 worldPos) {
            Vector2 localPoint;
            var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_tabletop.transform as RectTransform, screenPoint, Camera.main, out localPoint);

            return localPoint;
        }

        #endregion

        #region -- GET FREE POSITION ----------------------------

        public Vector2 GetFreePosWithDebug(DraggableToken token, Vector2 centerPos, int startIteration = -1, bool stackBothSides = true)
		{
#if DEBUG
            _currentDebug = new GameObject("ChoreoDebugInfo_" + token.name).AddComponent<ChoreographerDebugView>();
            _currentDebug.tabletop = _tabletop.transform;
            _currentDebug.targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);
            _currentDebug.checkedPoints = new List<Vector2>();
            _currentDebug.tokenOverlaps = false;
            _currentDebug.checkedRects = new List<Rect>();

            var pos = GetFreeTokenPosition(token, centerPos, startIteration, stackBothSides);
            _currentDebug.finalRect = GetCenterPosRect(pos, token.RectTransform.rect.size);
            _currentDebug.hasDebugData = true;

            _currentDebug.InitKill(10f); // In 10s, debug thing kills itself

            return pos;
#else
            return GetFreeTokenPosition(token, centerPos, startIteration);
#endif
        }

        Vector2 GetFreeTokenPosition(DraggableToken token, Vector2 centerPos, int startIteration = -1, bool stackBothSides = true)
		{
            //Debug.Log("Trying to find FREE POS for " + token.Id);
            centerPos = GetPosClampedToTable(centerPos);
			centerPos = SnapToGrid( centerPos );
			var targetRect = GetCenterPosRect(centerPos, token.RectTransform.rect.size);

            if (IsLegalPosition(targetRect, token))
                return centerPos;

            if (_currentDebug != null)
			{
                _currentDebug.targetRect = targetRect;
                _currentDebug.tokenOverlaps = true;
			}

            // We grab a bunch of test points
            startIteration = startIteration > 0f ? startIteration : 1;
            var currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, startIteration, maxGridIterations, stackBothSides);

            // Go over the test points and check if there's a clear spot to place things
            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token))
                    return point;
            }

            // we've exhausted things, so we do again
            // but we  shift the target pos a bit
            centerPos = centerPos + targetRect.size;
            // and we reduce the smaller rect size. This allows overlap
            targetRect.size = targetRect.size * 0.4f;

            //Debug.Log("Did not find a legal pos for " + token.Id + ", allowing for overlap!");

            // request a new set of points, since the center pos has shifted
            currentPoints = GetTestPoints(targetRect.position + targetRect.size / 2f, startIteration, maxGridIterations);

            if (IsLegalPosition(GetCenterPosRect(targetRect.position, targetRect.size), token))
                return centerPos;

            foreach (var point in currentPoints)
			{
                if (IsLegalPosition(GetCenterPosRect(point, targetRect.size), token))
                    return point;
            }

            NoonUtility.Log("Choreographer: No legal tabletop position found for " + token.EntityId + " (" + centerPos + ")!",1);

            return Vector2.zero;
        }

        // Tokens have their pos in their center, rects in the bottom right
        Rect GetCenterPosRect(RectTransform rectTrans)
		{
            return GetCenterPosRect(rectTrans.anchoredPosition, rectTrans.rect.size);
        }

        Rect GetCenterPosRect(Vector2 centerPos, Vector2 size)
		{
            return new Rect(centerPos - size / 2f, size);
        }

        Vector2 GetPosClampedToTable(Vector2 pos)
		{
            const float padding = .2f;

            pos.x = Mathf.Clamp(pos.x, tableRect.x + padding, tableRect.x + tableRect.width - padding);
            pos.y = Mathf.Clamp(pos.y, tableRect.y + padding, tableRect.y + tableRect.height - padding);
            return pos;
        }

        bool IsLegalPosition(Rect rect,  DraggableToken ignoreToken = null)
		{
            if (tableRect.Contains(rect.position + rect.size / 2f) == false)
                return false;
            
            Rect rectCheck;
            //Debug.Log("Checking if " + rect + " is a legal position");

            foreach (var token in _tabletop.GetTokens()) {
                rectCheck = GetCenterPosRect(token.RectTransform);

                if (CanTokenBeIgnored(token, ignoreToken))
                    continue;

                if (_currentDebug != null && !_currentDebug.checkedRects.Contains(rectCheck)) { 
                    _currentDebug.checkedRects.Add(rectCheck);
                    //Debug.Log("Checking for " + token.name + " at " + rectCheck);
                }

                if (rectCheck.Overlaps(rect)) {
                    //Debug.Log("Not a legal pos");
                    return false;
                }
            }

            //Debug.Log("IS a legal pos");
            return true;
        }

        bool CanTokenBeIgnored(DraggableToken token, DraggableToken ignoreToken) {
            if (token == ignoreToken)
                return true;
            if (token.IsBeingAnimated)
                return true;
            if (token.Defunct)
                return true;
            if (token.IsInAir)
                return true;
			if (token.NoPush)
				return true;

            return false;
        }

        Vector2[] GetTestPoints(Vector3 pos, int startIteration, int maxIteration, bool stackBothSides = true)
		{
            int numPoints = 0;
			// Always test in half-card intervals for best chance of finding valid slot
			float snap_x = 90.0f * 0.5f;
			float snap_y = 130.0f * 0.5f;

            for (int i = startIteration; i <= maxIteration; i++)
			{
                numPoints += 8 * i;
            }

            var points = new Vector2[numPoints];

            int p = 0;
            float y;
            float x;
            
            for (int v = 1 ; v < 2 + maxIteration * 2; v++)
			{
                y = (v % 2 == 0 ? -(v / 2) : (v / 2));

                for (int h = 1; h < 2 + maxIteration * 2; h++)
				{
                    if (h <= -1 + startIteration * 2 && v <= -1 + startIteration * 2)
                        continue; // don't put out points lower than our startIteration

					if (stackBothSides)
					{
						x = (h % 2 == 0 ? (h / 2) : -(h / 2));
					}
					else
					{
						x = h;
					}

                    points[p] = new Vector2(pos.x + x * snap_x, pos.y + y * snap_y);
					points[p] = SnapToGrid( points[p] );

                    if (_currentDebug != null) 
                        _currentDebug.checkedPoints.Add(points[p]);

                    p++;
                }
            }

            return points;
        }

        /*

        Vector2[] GetTestPoints(Vector3 pos, int gridIteration) {
            var points = new Vector2[8 * gridIteration];

            int p = 0;

            // right side points
            for (int i = -gridIteration; i <= gridIteration; i++) {
                points[p] = new Vector2(pos.x + gridIteration * pointGridSize, pos.y + i * pointGridSize);
                p++;
            }

            // left side points
            for (int i = -gridIteration; i <= gridIteration; i++) {
                points[p] = new Vector2(pos.x + -gridIteration * pointGridSize, pos.y + i * pointGridSize);
                p++;
            }

            // top side points
            for (int i = -gridIteration + 1; i <= gridIteration - 1; i++) {
                points[p] = new Vector2(pos.x + i * pointGridSize, pos.y + gridIteration * pointGridSize);
                p++;
            }

            // bottom side points
            for (int i = -gridIteration + 1; i <= gridIteration - 1; i++) {
                points[p] = new Vector2(pos.x + i * pointGridSize, pos.y + -gridIteration * pointGridSize);
                p++;
            }

            return points;
        }
        Vector2[] GetTestPoints(Vector3 pos, float radius) {
            float circumference = 2f * Mathf.PI * radius;
            int numPoints = Mathf.FloorToInt(circumference / checkPointPerArcLength);
            int remainder = numPoints % 4;

            if (remainder != 0) // making sure we're always a mulitple of 4
                numPoints += 4 - remainder;

            var points = new Vector2[numPoints];
            float angleSteps = Mathf.Deg2Rad * 360f / points.Length;

            for (int i = 0; i < points.Length; i++)
                points[i] = GetPointOnCircle(pos, radius, -i * angleSteps);

            return points;
        }

        Vector2 GetPointOnCircle(Vector3 origin, float radius, float angle) {
            return new Vector2(origin.x + radius * Mathf.Cos(angle),
                               origin.y + radius * Mathf.Sin(angle));
        }
        */

        #endregion



        public void SetGridSnapSize(float snapsize)
        {
            int snap = Mathf.RoundToInt(snapsize);
            switch (snap)
            {
                default:
                case 0: gridSnapSize = 0.0f; break;
                case 1: gridSnapSize = 1.0f; break;     // 1 card
                case 2: gridSnapSize = 0.5f; break;     // ½ card
                case 3: gridSnapSize = 0.25f; break;    // ¼ card
            }
        }

        public float GetGridSnapSize()
        {
            return gridSnapSize;
        }



        public Vector3 SnapToGrid(Vector3 v)
        {
            if (GetGridSnapSize() > 0f)
            {
                // Magical maths to snap cards to fractions of approx card dimensions - CP
                float snap_x = 90.0f * GetGridSnapSize();
                float snap_y = 130.0f * GetGridSnapSize();
                float recip_x = 1.0f / snap_x;
                float recip_y = 1.0f / snap_y;
                v.x *= recip_x; v.x = (float)Mathf.RoundToInt(v.x); v.x *= snap_x;
                v.y *= recip_y; v.y = (float)Mathf.RoundToInt(v.y); v.y *= snap_y;
            }
            return v;
        }

        public void WhenSettingUpdated(object newValue)
        {
            SetGridSnapSize(newValue is float ? (float)newValue : 0);
        }


        public void BeginNewSituation(SituationCreationCommand scc,List<ElementStackToken> withStacksInStorage) {
            if (scc.Recipe == null)
                throw new ApplicationException("DON'T PASS AROUND SITUATIONCREATIONCOMMANDS WITH RECIPE NULL");
            if(withStacksInStorage==null)
                throw new ApplicationException("WITHSTACKSINSTORAGE SHOULD NEVER BE NULL");

            //if new situation is beginning with an existing verb: do not action the creation.
            //oh: I could have an scc property which is a MUST CREATE override

            SituationController existingSituation;
            var sitToken = scc.SourceToken as SituationToken;

            if (sitToken != null && sitToken.EntityId==scc.Recipe.ActionId) { 
                existingSituation = sitToken.SituationController;
            }
            // We don't have a source token, then get us the first token with the appopriate id.
            else { 
                var registeredSits = Registry.Get<SituationsCatalogue>().GetRegisteredSituations();
                existingSituation = registeredSits.Find(sc => sc.situationToken.EntityId == scc.Recipe.ActionId);
            }

            //grabbing existingtoken: just in case some day I want to, e.g., add additional tokens to an ongoing one rather than silently fail the attempt.
            if (existingSituation != null) {
                if (existingSituation.Situation.State == SituationState.Complete && existingSituation.situationToken.IsTransient) {
                    //verb exists already, but it's completed. We don't want to block new temp verbs executing if the old one is complete, because
                    //otherwise there's an exploit to, e.g., leave hazard finished but unresolved to block new ones appearing.
                    //So nothing happens in this branch except logging.
                    NoonUtility.Log("Created duplicate verb, because previous one is both transient and complete.");
                }
                else {
                    NoonUtility.Log("Tried to create " + scc.Recipe.Id + " for verb " + scc.Recipe.ActionId + " but that verb is already active.");
                    //end execution here
                    return;
                }
            }
            
            var situationController = Registry.Get<SituationBuilder>().CreateSituation(scc);


            //if there's been (for instance) an expulsion, we now want to add the relevant stacks to this situation
            if (withStacksInStorage.Any())
                situationController.StoreStacks(withStacksInStorage);

            //if token has been spawned from an existing token, animate its appearance
            if (scc.SourceToken != null) {
                AnimateTokenTo(situationController.situationToken,
                    duration: 1f,
                    startPos: scc.SourceToken.RectTransform.anchoredPosition3D,
                    endPos: GetFreePosWithDebug(situationController.situationToken, scc.SourceToken.RectTransform.anchoredPosition, 3),
                    startScale: 0f,
                    endScale: 1f);
            }
            else {
                Registry.Get<Choreographer>().ArrangeTokenOnTable(situationController.situationToken, null);
            }
        }


        #region -- ANIMATIONS ------------------------

        void AnimateTokenTo(DraggableToken token, float duration, Vector3 startPos, Vector3 endPos, float startScale = 1f, float endScale = 1f) {
            var tokenAnim = token.gameObject.AddComponent<TokenAnimation>();
            tokenAnim.onAnimDone += SituationAnimDone;
            tokenAnim.SetPositions(startPos, endPos);
            tokenAnim.SetScaling(startScale, endScale);
            tokenAnim.StartAnim(duration);
        }

        public void PrepareElementForSendAnim(ElementStackToken stack, SituationToken ownerSituation)
		{
            _tabletop.GetElementStacksManager().AcceptStack(stack, new Context(Context.ActionSource.DoubleClickSend)); // this reparents, sets container
            //_tabletop.DisplayHere(stack as Core.Interfaces.ElementStackToken); // this reparents, sets container
            //stack.transform.position = ownerSituation.transform.position;
            stack.FlipToFaceUp(true);
        }

        public void PrepareElementForGreedyAnim(ElementStackToken stack, SituationToken ownerSituation)
		{
            _tabletop.GetElementStacksManager().AcceptStack(stack, new Context(Context.ActionSource.GreedySlot)); // this reparents, sets container
            //_tabletop.DisplayHere(stack as Core.Interfaces.ElementStackToken); // this reparents, sets container
            stack.transform.position = ownerSituation.transform.position;
            stack.FlipToFaceUp(true);
        }

        public void MoveElementToSituationSlot(ElementStackToken stack, TokenAndSlot tokenSlotPair, Action<ElementStackToken, TokenAndSlot> callOnAnimDone, float durationOverride = -1.0f)
		{
            var startPos = stack.RectTransform.anchoredPosition3D;
            var endPos = tokenSlotPair.Token.GetOngoingSlotPosition();
            float distance = Vector3.Distance(startPos, endPos);
            float duration = durationOverride>0.0f ? durationOverride : Mathf.Max(0.3f, distance * 0.001f);

            var stackAnim = stack.gameObject.AddComponent<TokenAnimationToSlot>();
            stackAnim.onElementSlotAnimDone += callOnAnimDone;
            stackAnim.SetPositions(startPos, endPos);
            stackAnim.SetScaling(1f, 0.35f);
            stackAnim.SetTargetSlot(tokenSlotPair);

            tokenSlotPair.RecipeSlot.IsBeingAnimated = true;

            stackAnim.StartAnim(duration);
        }

        public void ElementSendAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair)
		{
            try
            {
				if (tokenSlotPair.RecipeSlot.Equals(null) ||
					(tokenSlotPair.Token.SituationController.situationWindow.GetStartingSlots().Contains( tokenSlotPair.RecipeSlot )==false &&
					 tokenSlotPair.Token.SituationController.situationWindow.GetOngoingSlots().Contains( tokenSlotPair.RecipeSlot )==false))
				{
					// Abort - either the slot has gone or it's been removed from the valid list
					element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
				}
				else
				{
					// Assign element to new slot
					tokenSlotPair.RecipeSlot.AcceptStack(element, new Context(Context.ActionSource.AnimEnd));
					if (!tokenSlotPair.Token.SituationController.IsOpen)
						tokenSlotPair.Token.OpenSituation();
				}
				// Clear this whether the card arrived successfully or not, otherwise slot is locked for rest of session - CP
				tokenSlotPair.RecipeSlot.IsBeingAnimated = false;
            }
            catch
            {
				// If anything goes wrong just dump the card back on the desk
				element.ReturnToTabletop(new Context(Context.ActionSource.PlayerDrag));
            }
        }

        public void ElementGreedyAnimDone(ElementStackToken element, TokenAndSlot tokenSlotPair)
		{
            if (tokenSlotPair.RecipeSlot.Equals(null))
                return;

            tokenSlotPair.RecipeSlot.AcceptStack(element, new Context(Context.ActionSource.AnimEnd));
            tokenSlotPair.RecipeSlot.IsBeingAnimated = false;
        }

        void SituationAnimDone(SituationToken token) {
            _tabletop.DisplaySituationTokenOnTable(token, new Context(Context.ActionSource.AnimEnd));
        }

        #endregion


    }
}
