using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class gameMan : MonoBehaviour
{
    public bool runCheck = false;

    public InputActionAsset inputAss;
    public Transform backgroundParent;
    public Transform cellParent;
    public Transform borderParent;
    public Transform itemsParent;
    public Transform foodParent;
    public Transform ghostParent;
    public List<RawImage> cellBackgrounds = new List<RawImage>();
    public List<cellSquare> gridCells = new List<cellSquare>();
    public List<RawImage> gridBorders = new List<RawImage>();
    public List<RawImage> foodSquares = new List<RawImage>();
    public List<foodSquare> foodType = new List<foodSquare>();
    public List<RawImage> ghostFoods = new List<RawImage>();
    public List<int> vertOrder = new List<int>();
    private InputAction inputUp;
    private InputAction inputDown;
    private InputAction inputLeft;
    private InputAction inputRight;
    private InputAction inputConfirm;
    private InputAction inputCancel;

    public Color32 cellSelect;
    public Color32 cellValid;
    public Color32 cellInvalid;
    public Color32 cellMove;

    public int currentCellIndex;
    private int gridSize;
    private int foodTypes = 4; // Number of different food types
    private int potFoodMatch = -1;
    private int lastMouseEnter;
    public int mouseCellEnter;
    public int grabIndex = -1;
    public int grabbedFood = -1;
    public bool ghostFoodPlacer = false;
    public Color32 ghostOn;
    public Color32 ghostOff;
    public Texture emptyTexture;
    private List<int> lastMatchCount = new List<int>();
    public Text txtMoves;
    public Text txtHunger;
    private int numMoves;
    private int numHunger;
    public Color32 cellNormal;
    public Color32 cellMatch;
    public throwObjects thrower;
    private float dropDelay = 0.2f;
    private bool doRecheck = false;
    public List<int> cravings = new List<int>();
    public foodCrave FoodCrave;
    private int currentCrave = -1;
    public GameObject endGame;
    //   public Text txtDebug;

    void OnEnable()
    {
        Cursor.visible = true;
        inputUp = inputAss.FindAction("menuUp");
        inputDown = inputAss.FindAction("menuDown");
        inputLeft = inputAss.FindAction("menuLeft");
        inputRight = inputAss.FindAction("menuRight");
        inputConfirm = inputAss.FindAction("menuConfirm");
        inputCancel = inputAss.FindAction("menuClose");
        inputUp.performed += ctx => MoveSelection(Vector2.up);
        inputDown.performed += ctx => MoveSelection(Vector2.down);
        inputLeft.performed += ctx => MoveSelection(Vector2.left);
        inputRight.performed += ctx => MoveSelection(Vector2.right);
        inputAss.Enable();

        numMoves = 12;
        resetGrid();
        foreach (Transform background in backgroundParent) { cellBackgrounds.Add(background.GetComponent<RawImage>()); }
        foreach (Transform cell in cellParent) { gridCells.Add(cell.GetComponent<cellSquare>()); }
        foreach (Transform border in borderParent) { gridBorders.Add(border.GetComponent<RawImage>()); }
        foreach (Transform item in itemsParent) { foodSquares.Add(item.GetComponent<RawImage>()); }
        foreach (Transform foodItem in foodParent) { foodType.Add(foodItem.GetComponent<foodSquare>()); }
        foreach (Transform ghostFruit in ghostParent) { ghostFoods.Add(ghostFruit.GetComponent<RawImage>()); }

        gridSize = System.Convert.ToInt32(Mathf.Sqrt(gridCells.Count));

        for (int i = 0; i < gridCells.Count; i++)
        {
            int row = i / gridSize;  // Calculate the row number
            int col = i % gridSize;  // Calculate the column number

            gridCells[i].cellIndex = i;
            gridCells[i].rowNumber = row;
            gridCells[i].colNumber = col;

            if (col == 0) { gridCells[i].canLeft = false; }
            if (col == (gridSize - 1)) { gridCells[i].canRight = false; }
            if (row == 0) { gridCells[i].canUp = false; }
            if (row == (gridSize - 1)) { gridCells[i].canDown = false; }
        }

        cravings.Clear();
        int crave = 0;

        // Populate the list with indices
        while (crave < foodType.Count)
        {
            cravings.Add(crave);
            crave++;
        }

        // Shuffle the list using Random
        System.Random rng = new System.Random();
        cravings = cravings.OrderBy(x => rng.Next()).ToList();

        foodTypes = foodType.Count;
        currentCellIndex = 0;
        buildVert();
        GenerateGameBoard();
        HighlightCurrentCellBorders();
        ShowMoves();
    }

    void buildVert()
    {
        vertOrder.Clear();
        int sqRoot = System.Convert.ToInt32(Mathf.Sqrt(gridCells.Count));
        int colPos = 0;
        int rowPos = 0;
        int cellPos = 0;
        while (colPos < sqRoot)
        {
            while (rowPos < sqRoot)
            {
                cellPos = colPos + (rowPos * sqRoot);
                vertOrder.Add(cellPos);
                rowPos++;
            }
            colPos++;
            rowPos = 0;
        }
    }

    private void Update()
    {
        txtHunger.text = numHunger.ToString();
        txtMoves.text = numMoves.ToString();

        // Mouse cell selection
        if (mouseCellEnter > -1 && mouseCellEnter != lastMouseEnter)
        {
            if (grabIndex == -1)
            {
                currentCellIndex = mouseCellEnter;
                HighlightCurrentCellBorders();
            }
            else
            {
                currentCellIndex = mouseCellEnter;
                MoveSelectionWithMouse();
                ShowGhostPlacement(grabbedFood);
            }
        }

        lastMouseEnter = mouseCellEnter;

        if (inputConfirm.WasPressedThisFrame())
        {
            if (grabIndex == -1)
            {
                // Grab food
                ghostFoodPlacer = true;
                grabbedFood = gridCells[currentCellIndex].foodItem;
                foodSquares[currentCellIndex].texture = emptyTexture;
                ghostFoods[currentCellIndex].texture = foodType[grabbedFood].foodImage;
                ghostFoods[currentCellIndex].color = ghostOn;
                grabIndex = currentCellIndex;
            }
            else
            {
                // Place food
                //           Debug.Log("Place fruit: grabbedFood:" + grabbedFood + " , grabIndex:" + grabIndex);
                int destFood = gridCells[currentCellIndex].foodItem;
                gridCells[grabIndex].foodItem = destFood;
                gridCells[currentCellIndex].foodItem = grabbedFood;
                grabIndex = -1; grabbedFood = -1;
                ghostFoodPlacer = false;
                ghostFoods[currentCellIndex].texture = emptyTexture;
                ghostFoods[currentCellIndex].color = ghostOff;
                numMoves--;
      //          checkCravings();
                updateBoard();
                ShowMoves();
                StartCoroutine(playerMatch());
            }
        }
        if (inputCancel.WasPressedThisFrame())
        {
            if (grabIndex > -1)
            {
                ghostFoodPlacer = false;
                gridCells[grabIndex].foodItem = grabbedFood;
                foodSquares[grabIndex].texture = foodType[grabbedFood].foodImage;
                ghostFoods[currentCellIndex].texture = emptyTexture;
                ghostFoods[currentCellIndex].color = ghostOff;
                currentCellIndex = grabIndex;
                grabIndex = -1;
                grabbedFood = -1;
            }
        }
        //        txtDebug.text = "currentCellIndex: " + currentCellIndex.ToString() + " , ghostFoodPlacer: " + ghostFoodPlacer;
 
        if (runCheck) { StartCoroutine(sanityCheck()); }
    }

    void checkCravings()
    {
        // List of moves where cravings should be applied
        int[] cravingMoves = { 3, 5, 8, 10 };
   //     int[] cravingMoves = { 2, 4, 7, 9 };

        // Find the index of the current move in the cravingMoves array
        int cravingIndex = Array.IndexOf(cravingMoves, numMoves);

        // Check if numMoves is within the allowed range (2, 5, 8, 11) and within cravings list bounds
        if (cravingIndex != -1 && cravingIndex < cravings.Count && numMoves <= 11)
        {
            currentCrave = cravings[cravingIndex];
            // Apply the corresponding craving
            FoodCrave.currentCrave = cravings[cravingIndex];
            FoodCrave.gameObject.SetActive(true);
        }
        else
        {
            currentCrave = -1;
            // Disable cravings if the move is not applicable or if numMoves exceeds 11
            FoodCrave.gameObject.SetActive(false);
        }
    }

    private void MoveSelectionWithMouse()
    {
        List<int> validMoveCells = new List<int>();
        validMoveCells.Clear();
        validMoveCells.Add(grabIndex - 1);
        validMoveCells.Add(grabIndex + 1);
        validMoveCells.Add(grabIndex - gridSize);
        validMoveCells.Add(grabIndex + gridSize);
        bool isValid = false;
        int index = 0;
        while (index < validMoveCells.Count)
        {
            if (currentCellIndex == validMoveCells[index]) { isValid = true; }
            index++;
        }
        if (!isValid) { currentCellIndex = grabIndex; }
    }

    private bool IsWithinOneCellRange(int originalIndex, int newIndex)
    {
        // Check if the new index is one step away from the original in any direction
        return (newIndex == originalIndex - gridSize ||  // One step up
                newIndex == originalIndex + gridSize ||  // One step down
                newIndex == originalIndex - 1 ||         // One step left
                newIndex == originalIndex + 1);          // One step right
    }

    IEnumerator playerMatch()
    {
        doRecheck = false;
        List<int> hCheck = getHmatches();
        if (hCheck.Count > 0)
        {
            int matchIndex = 0;
            while (matchIndex < hCheck.Count)
            {
                int getLength = lastMatchCount[matchIndex];
                int aboveThree = getLength - 3;
                numMoves = numMoves + aboveThree;
                int currentPoints = 3 + (3 * aboveThree);

                if (currentCrave > -1 & gridCells[hCheck[matchIndex]].foodItem == currentCrave)
                {
    //                Debug.Log("Double points awarded for craving!");
                    currentPoints = currentPoints * 2;
                }
                numHunger = numHunger + currentPoints;

                checkCravings();

                int iterIndex = 0; int startIndex = hCheck[matchIndex];
                while (iterIndex < lastMatchCount[matchIndex])
                {
                    cellBackgrounds[startIndex + iterIndex].color = cellMatch;
                    //         Debug.Log("Color cell: " + (startIndex + iterIndex) + " for count: " + iterIndex + " of " + lastMatchCount[matchIndex]);
                    thrower.throwThese.Add(foodSquares[startIndex + iterIndex].gameObject);

                    // Wait for the object to be thrown before changing the texture
                    thrower.queued = true;
                    while (thrower.queued)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    foodSquares[startIndex + iterIndex].texture = emptyTexture;
   //                 gridCells[startIndex + iterIndex].foodItem = -1;
                    iterIndex++;
                }
                yield return new WaitForSeconds(0.5f);
                matchIndex++;
            }
        }

        List<int> vCheck = getVmatches();
        if (vCheck.Count > 0)
        {
            int matchIndex = 0;
            while (matchIndex < vCheck.Count)
            {
                int getLength = lastMatchCount[matchIndex];
                int aboveThree = getLength - 3;
                numMoves = numMoves + aboveThree;
                int currentScore = 3 + (3 * aboveThree);

                if (currentCrave > -1 & gridCells[vCheck[matchIndex]].foodItem == currentCrave)
                {
        //            Debug.Log("Double points awarded for craving!");
                    currentScore = currentScore * 2;
                }
                checkCravings();

                numHunger = numHunger + currentScore;
                int iterIndex = 0; int startIndex = vCheck[matchIndex];
                int perIteration = 1;
                //            Debug.Log("vCheck set startIndex to: " + startIndex + " , matchCount:" + lastMatchCount.Count);
                while (perIteration < lastMatchCount[matchIndex] + 1)
                {
                    cellBackgrounds[startIndex + iterIndex].color = cellMatch;
                    //                 Debug.Log("Color cell: " + (startIndex + iterIndex) + " for count: " + perIteration + " of " + lastMatchCount[matchIndex]);

                    thrower.throwThese.Add(foodSquares[startIndex + iterIndex].gameObject);

                    // Wait for the object to be thrown before changing the texture
                    thrower.queued = true;
                    while (thrower.queued)
                    {
                        yield return new WaitForEndOfFrame();
                    }

                    foodSquares[startIndex + iterIndex].texture = emptyTexture;
 //                   gridCells[startIndex + iterIndex].foodItem = -1;
                    iterIndex = gridSize * perIteration;
                    perIteration++;
                }
                yield return new WaitForSeconds(0.5f);
                matchIndex++;
            }
        }

        yield return new WaitForSeconds(dropDelay);

        int cellIndex = 0;
        foreach (cellSquare cell in gridCells)
        {
            if (foodSquares[cellIndex].GetComponent<RawImage>().texture == emptyTexture)
            {
                cell.foodItem = -1;
            }
            cellIndex++;
        }

        if (numMoves == 0 & hCheck.Count == 0 & vCheck.Count == 0)
        {
            endGame.SetActive(true);
            endGame.transform.Find("txtScore").GetComponent<Text>().text = numHunger.ToString();
        }
        else
        {
            StartCoroutine(DropFoodAfterMatches());
        }
    }

    IEnumerator DropFoodAfterMatches()
    {
        // Step 1: Calculate Drop Distances
        for (int column = 0; column < gridSize; column++)
        {
            int emptySpacesBelow = 0;

            for (int row = gridSize - 1; row >= 0; row--)
            {
                int cellIndex = row * gridSize + column;

                if (gridCells[cellIndex].foodItem == -1)
                {
                    // Count empty spaces below
                    emptySpacesBelow++;
                }
                else if (emptySpacesBelow > 0)
                {
                    // Set the drop distance for this cell
                    gridCells[cellIndex].dropDistance = emptySpacesBelow;
                }
                else
                {
                    // Reset drop distance if no empty spaces are found below
                    gridCells[cellIndex].dropDistance = 0;
                }
            }
        }

        // Step 2: Apply Drops Simultaneously
        List<IEnumerator> dropCoroutines = new List<IEnumerator>();

        float delay = 0.1f;
        for (int column = 0; column < gridSize; column++)
        {
            for (int row = gridSize - 1; row >= 0; row--)
            {
                int cellIndex = row * gridSize + column;
                int dropDistance = gridCells[cellIndex].dropDistance;

                if (dropDistance > 0)
                {
                    int targetCell = cellIndex + (dropDistance * gridSize);

                    if (targetCell < gridSize * gridSize)
                    {
                        Vector2 originalPos = foodSquares[cellIndex].rectTransform.localPosition;
                        Vector2 targetPos = foodSquares[targetCell].rectTransform.localPosition;

                        dropCoroutines.Add(MoveFood(cellIndex, targetCell, originalPos, targetPos));
                        delay = 0.22f;
                    }
                }
            }
        }

        // Execute all drops simultaneously
        foreach (var dropCoroutine in dropCoroutines)
        {
            StartCoroutine(dropCoroutine);
        }

        // Wait for all drops to finish
        foreach (var dropCoroutine in dropCoroutines)
        {
            while (dropCoroutine.MoveNext())
            {
                yield return null;
            }
        }

        // Step 3: Reset Drop Distances and Clear Empty Cells
        for (int column = 0; column < gridSize; column++)
        {
            for (int row = gridSize - 1; row >= 0; row--)
            {
                int cellIndex = row * gridSize + column;
                gridCells[cellIndex].dropDistance = 0;

                if (gridCells[cellIndex].foodItem == -1)
                {
                    foodSquares[cellIndex].GetComponent<RawImage>().texture = emptyTexture;
                }
            }
        }
        StartCoroutine(FillEmptyCells(delay));
    }

    IEnumerator MoveFood(int sourceCell, int targetCell, Vector2 originalPos, Vector2 targetPos)
    {
        float dropSpeed = 366f;
        float elapsedTime = 0f;
        float distance = Vector2.Distance(originalPos, targetPos);

        while (elapsedTime < distance / dropSpeed)
        {
            foodSquares[sourceCell].rectTransform.localPosition = Vector2.Lerp(originalPos, targetPos, (elapsedTime * dropSpeed) / distance);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Finalize the drop by setting the target cell's position
        foodSquares[sourceCell].rectTransform.localPosition = targetPos;
        gridCells[targetCell].foodItem = gridCells[sourceCell].foodItem;
        foodSquares[targetCell].GetComponent<RawImage>().texture = foodType[gridCells[targetCell].foodItem].foodImage;

        // Clear the source cell
        gridCells[sourceCell].foodItem = -1;
        foodSquares[sourceCell].GetComponent<RawImage>().texture = emptyTexture;

        // Reset the source cell's position
        foodSquares[sourceCell].rectTransform.localPosition = originalPos;

        doRecheck = true;
    }

    void updateBoard()
    {
        int index = 0;
        foreach (cellSquare getSquare in gridCells)
        {
            foodSquares[index].texture = foodType[getSquare.foodItem].foodImage;
            index++;
        }
    }

    void resetGrid()
    {
        foreach (Transform cell in cellParent)
        {
            cellSquare getCell = cell.GetComponent<cellSquare>();
            getCell.canDown = true;
            getCell.canUp = true;
            getCell.canLeft = true;
            getCell.canRight = true;
            getCell.foodItem = -1; // Reset food items
        }
        foreach (Transform border in borderParent)
        {
            RawImage getBorder = border.GetComponent<RawImage>();
            getBorder.color = cellInvalid;
        }
        foreach (RawImage background in cellBackgrounds)
        {
            background.color = cellNormal;
        }
    }

    void MoveSelection(Vector2 direction)
    { // Movement for gamepad only
        int newCellIndex = currentCellIndex;
        cellSquare currentCell = gridCells[currentCellIndex];

        if (grabIndex == -1)
        {
            if (direction == Vector2.up && currentCell.canUp) newCellIndex -= gridSize;
            if (direction == Vector2.down && currentCell.canDown) newCellIndex += gridSize;
            if (direction == Vector2.left && currentCell.canLeft) newCellIndex -= 1;
            if (direction == Vector2.right && currentCell.canRight) newCellIndex += 1;
        }
        if (grabIndex > -1)
        {
            if (direction == Vector2.up && currentCell.canUp) newCellIndex -= gridSize;
            if (direction == Vector2.down && currentCell.canDown) newCellIndex += gridSize;
            if (direction == Vector2.left && currentCell.canLeft) newCellIndex -= 1;
            if (direction == Vector2.right && currentCell.canRight) newCellIndex += 1;

            if (direction == Vector2.up)
            {
                if (newCellIndex != (grabIndex - gridSize) && grabIndex != newCellIndex) { newCellIndex = currentCellIndex; }
            }
            if (direction == Vector2.down)
            {
                if (newCellIndex != (grabIndex + gridSize) && grabIndex != newCellIndex) { newCellIndex = currentCellIndex; }
            }
            if (direction == Vector2.left)
            {
                if (newCellIndex != (grabIndex - 1) && grabIndex != newCellIndex) { newCellIndex = currentCellIndex; }
            }
            if (direction == Vector2.right)
            {
                if (newCellIndex != (grabIndex + 1) && grabIndex != newCellIndex) { newCellIndex = currentCellIndex; }
            }
        }

        if (newCellIndex != currentCellIndex)
        {
            currentCellIndex = newCellIndex;
            if (!ghostFoodPlacer)
            {
                HighlightCurrentCellBorders();
            }
            else
            {
                ShowGhostPlacement(grabbedFood);
            }
        }
    }

    void ShowGhostPlacement(int foodItem)
    {
        //     UnityEngine.Debug.Log("Ghost placer is running!  foodItem: " + foodItem + " , currentCell: " + currentCellIndex);

        foreach (RawImage cell in ghostFoods)
        {
            cell.color = ghostOff;
            cell.texture = emptyTexture;
        }
        ghostFoods[currentCellIndex].color = ghostOn;
        ghostFoods[currentCellIndex].texture = foodType[foodItem].foodImage;
    }

    void ShowMoves()
    {
        int index = 0;
        while (index < gridCells.Count)
        {
            int numMatches = 0;
            List<int> checkNeighbours = getNeighbours(index);

            foreach (int getCellIndex in checkNeighbours)
            {
                potFoodMatch = -1;
                bool wouldMatch = checkPotentialMatch(index, getCellIndex);
                if (wouldMatch & potFoodMatch == gridCells[getCellIndex].foodItem)
                {
                    foodSquares[getCellIndex].GetComponent<Animator>().Play("foodShake");
                    numMatches++;
                }
                if (numMatches == 0 & !wouldMatch & potFoodMatch != gridCells[getCellIndex].foodItem)
                {
                    foodSquares[getCellIndex].GetComponent<Animator>().Play("foodNormal");
                }
            }
            index++;
        }
    }

    IEnumerator FillEmptyCells(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < gridCells.Count; i++)
        {
            cellBackgrounds[i].color = cellNormal;
            // Check if the cell is empty (i.e., has no food item)
            if (gridCells[i].foodItem == -1)
            {
                int fruitTypes;
                do
                {
                    fruitTypes = UnityEngine.Random.Range(0, foodTypes);
                }
                while (WouldCreateMatch(i, fruitTypes));

                gridCells[i].foodItem = fruitTypes;
                foodSquares[i].texture = foodType[fruitTypes].foodImage;
            }
        }
        if (doRecheck) { StartCoroutine(playerMatch()); }
    }

    void GenerateGameBoard()
    {
        do
        {
            FillBoard();
        }
        while (!HasValidMove() || HasInitialMatches());
    }

    void FillBoard()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            int fruitTypes;
            do
            {
                fruitTypes = UnityEngine.Random.Range(0, foodTypes);
            }
            while (WouldCreateMatch(i, fruitTypes));

            gridCells[i].foodItem = fruitTypes;
            gridCells[i].foodItem = fruitTypes;
            foodSquares[i].texture = foodType[fruitTypes].foodImage;
        }
    }

    bool WouldCreateMatch(int index, int foodType)
    {
        int row = index / gridSize;
        int col = index % gridSize;

        // Check horizontal match
        if (col >= 2 &&
            gridCells[index - 1].foodItem == foodType &&
            gridCells[index - 2].foodItem == foodType)
        {
            return true;
        }
        if (col < gridSize - 2 &&
            gridCells[index + 1].foodItem == foodType &&
            gridCells[index + 2].foodItem == foodType)
        {
            return true;
        }
        if (col >= 1 && col < gridSize - 1 &&
            gridCells[index - 1].foodItem == foodType &&
            gridCells[index + 1].foodItem == foodType)
        {
            return true;
        }

        // Check vertical match
        if (row >= 2 &&
            gridCells[index - gridSize].foodItem == foodType &&
            gridCells[index - 2 * gridSize].foodItem == foodType)
        {
            return true;
        }
        if (row < gridSize - 2 &&
            gridCells[index + gridSize].foodItem == foodType &&
            gridCells[index + 2 * gridSize].foodItem == foodType)
        {
            return true;
        }
        if (row >= 1 && row < gridSize - 1 &&
            gridCells[index - gridSize].foodItem == foodType &&
            gridCells[index + gridSize].foodItem == foodType)
        {
            return true;
        }

        return false;
    }

    bool HasInitialMatches()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            if (IsMatch(i))
            {
                return true;
            }
        }
        return false;
    }

    bool IsMatch(int index)
    {
        int row = index / gridSize;
        int col = index % gridSize;
        int foodType = gridCells[index].foodItem;

        // Check horizontal match
        if (col <= gridSize - 3 &&
            gridCells[index + 1].foodItem == foodType &&
            gridCells[index + 2].foodItem == foodType)
        {
            return true;
        }
        // Check vertical match
        if (row <= gridSize - 3 &&
            gridCells[index + gridSize].foodItem == foodType &&
            gridCells[index + 2 * gridSize].foodItem == foodType)
        {
            return true;
        }

        return false;
    }

    bool HasValidMove()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            if (CanSwapCreateMatch(i, Vector2.up) ||
                CanSwapCreateMatch(i, Vector2.down) ||
                CanSwapCreateMatch(i, Vector2.left) ||
                CanSwapCreateMatch(i, Vector2.right))
            {
                return true;
            }
        }
        return false;
    }

    bool CanSwapCreateMatch(int index, Vector2 direction)
    {
        int targetIndex = GetIndexFromDirection(index, direction);
        if (targetIndex < 0 || targetIndex >= gridCells.Count) return false;

        // Swap the items
        int temp = gridCells[index].foodItem;
        gridCells[index].foodItem = gridCells[targetIndex].foodItem;
        gridCells[targetIndex].foodItem = temp;

        bool isMatch = IsMatch(index) || IsMatch(targetIndex);

        // Swap back
        temp = gridCells[index].foodItem;
        gridCells[index].foodItem = gridCells[targetIndex].foodItem;
        gridCells[targetIndex].foodItem = temp;

        return isMatch;
    }

    int GetIndexFromDirection(int index, Vector2 direction)
    {
        int newRow = index / gridSize + (int)direction.y;
        int newCol = index % gridSize + (int)direction.x;
        if (newRow < 0 || newRow >= gridSize || newCol < 0 || newCol >= gridSize)
        {
            return -1; // Invalid index
        }
        return newRow * gridSize + newCol;
    }

    void HighlightCurrentCellBorders()
    {
        // Reset all borders to invalid color
        for (int i = 0; i < gridBorders.Count; i++)
        {
            gridBorders[i].color = cellInvalid;
            gridCells[i].willDest = false;
        }

        cellSquare currentCell = gridCells[currentCellIndex];
        gridBorders[currentCellIndex].color = cellSelect;

        // Highlight potential movement directions
        if (currentCell.canUp)
        {
            int upIndex = currentCellIndex - gridSize;
            gridBorders[upIndex].color = cellValid;
        }
        if (currentCell.canDown)
        {
            int downIndex = currentCellIndex + gridSize;
            gridBorders[downIndex].color = cellValid;
        }
        if (currentCell.canLeft)
        {
            int leftIndex = currentCellIndex - 1;
            gridBorders[leftIndex].color = cellValid;
        }
        if (currentCell.canRight)
        {
            int rightIndex = currentCellIndex + 1;
            gridBorders[rightIndex].color = cellValid;
        }
        List<int> checkNeighbours = getNeighbours(currentCellIndex);
        foreach (int getCellIndex in checkNeighbours)
        {
            bool wouldMatch = checkPotentialMatch(currentCellIndex, getCellIndex);
            if (wouldMatch)
            {
                gridBorders[getCellIndex].color = cellMove;
                gridCells[getCellIndex].willDest = true;
            }
        }
        checkNeighbours.Clear();
    }

    bool WouldSwapCreateMatch(int index, Vector2 direction)
    {
        int targetIndex = GetIndexFromDirection(index, direction);

        Debug.Log("Current index: " + index + " , Direction: " + direction + " , Target: " + targetIndex);

        if (targetIndex < 0 || targetIndex >= gridCells.Count) return false;

        // Swap the items
        int temp = gridCells[index].foodItem;
        gridCells[index].foodItem = gridCells[targetIndex].foodItem;
        gridCells[targetIndex].foodItem = temp;

        bool isMatch = IsMatch(index) || IsMatch(targetIndex);

        // Swap back
        gridCells[targetIndex].foodItem = gridCells[index].foodItem;
        gridCells[index].foodItem = temp;

        return isMatch;
    }

    List<int> getHmatches()
    {
        lastMatchCount.Clear();
        List<int> matchPlaces = new List<int>();
        int index = 0;
        int iterations = 0;
        int lastFood = -1;
        int colCount = 0;

        while (index < gridCells.Count)
        {
            // Horizontal
            int checkFood = gridCells[index].foodItem;
            if (checkFood == lastFood)
            {
                iterations++;
                if (iterations > 2)
                {
                    //                  Debug.Log("iteration:" + iterations + " mPlaceCount:" + matchPlaces.Count);
                    if (matchPlaces.Count > 0 && matchPlaces[matchPlaces.Count - 1] == index - (iterations - 1))
                    {
                        // If match is > 3, update count
                        lastMatchCount[lastMatchCount.Count - 1] = iterations;
                        potFoodMatch = checkFood;
                    }
                    else
                    {
                        // Standard match 3
                        matchPlaces.Add(index - (iterations - 1));
                        lastMatchCount.Add(iterations);
                        potFoodMatch = checkFood;
                    }
                }
            }
            else
            {
                iterations = 1; // Reset iterations for a new match sequence
            }

            index++;
            colCount++;
            if (colCount == gridSize)
            {
                iterations = 0; // Reset iterations for a new row
                colCount = 0;
                lastFood = -1; // Reset the lastFood for the new row
            }
            else
            {
                lastFood = checkFood; // Update lastFood for the current row
            }
        }

        return matchPlaces;
    }

    List<int> getVmatches()
    {
        lastMatchCount.Clear();
        List<int> matchPlaces = new List<int>();
        int index = 0;
        int colCount = 0;
        int iterations = 0;
        int lastFood = -1;
        int cellReference = 0;
        while (index < gridCells.Count)
        { // Vertical
            cellReference = vertOrder[index];
            int checkFood = gridCells[vertOrder[index]].foodItem;
            if (checkFood == lastFood)
            {
                iterations++;
                if (iterations > 2)
                {
                    //                  Debug.Log("iteration:" + iterations + " mPlaceCount:" + matchPlaces.Count);
                    if (matchPlaces.Count > 0 && matchPlaces[matchPlaces.Count - 1] == cellReference - (iterations - 1) * gridSize)
                    { // Match is > 3 update count
                        lastMatchCount[lastMatchCount.Count - 1] = iterations;
                        potFoodMatch = checkFood;
                    }
                    else
                    { // Match 3 found
                        int newStart = cellReference - (iterations - 1) * gridSize;
                        matchPlaces.Add(newStart);
                        lastMatchCount.Add(iterations);
                        potFoodMatch = checkFood;
                    }
                }
            }
            else
            {
                iterations = 1; // Reset iterations for a new match sequence
            }
            index++;
            colCount++;
            lastFood = checkFood;
            if (colCount == gridSize)
            {
                colCount = 0;
                iterations = 0;
                lastFood = -1; // Reset the lastFood for the new column
            }
        }
        return matchPlaces;
    }

    bool checkPotentialMatch(int cellIndex, int swapIndex)
    {
        bool isMatch = false;
        List<int> tempFoodItems = new List<int>();
        int index = 0;
        while (index < gridCells.Count)
        {
            if (index == cellIndex)
            {
                tempFoodItems.Add(gridCells[swapIndex].foodItem);
            }
            else if (index == swapIndex)
            {
                tempFoodItems.Add(gridCells[cellIndex].foodItem);
            }
            else
            {
                tempFoodItems.Add(gridCells[index].foodItem);
            }
            index++;
        }
        index = 0;
        int iterations = 0; int lastFood = -1; bool thisCheck = false; int colCount = 0;
        while (index < gridCells.Count)
        { // Horizontal
            int checkFood = tempFoodItems[index];
            if (checkFood == lastFood)
            {
                iterations++;
                if (iterations > 2) { thisCheck = true; potFoodMatch = checkFood; }
            }
            else { iterations = 1; }
            index++; colCount++;
            if (colCount == gridSize) { iterations = 0; colCount = 0; }
            lastFood = checkFood;
        }
        if (thisCheck) { isMatch = true; }
        index = 0; int vStart = 0;
        iterations = 0; lastFood = -1; thisCheck = false; cellIndex = 0;
        while (index < gridCells.Count)
        { // Vertical

            int checkFood = tempFoodItems[vertOrder[index]];
            if (checkFood == lastFood) { iterations++; if (iterations > 2) { thisCheck = true; potFoodMatch = checkFood; } }
            else { iterations = 1; }
            index++; colCount++;
            lastFood = checkFood;
            if (colCount == gridSize) { colCount = 0; iterations = 0; }
        }
        if (thisCheck) { isMatch = true; }
        tempFoodItems.Clear();
        return isMatch;
    }

    List<int> getNeighbours(int cellIndex)
    {
        //      Debug.Log("Getting neighbours list");
        List<int> list = new List<int>();
        cellSquare getCell = gridCells[cellIndex];
        if (getCell.canDown) { list.Add(cellIndex + gridSize); }
        if (getCell.canUp) { list.Add(cellIndex - gridSize); }
        if (getCell.canLeft) { list.Add(cellIndex - 1); }
        if (getCell.canRight) { list.Add(cellIndex + 1); }
        return list;
    }

    IEnumerator sanityCheck()
    {
        runCheck = false;
        int index = 0;
        foreach (RawImage getImg in foodSquares)
        {
            int getFood = gridCells[index].foodItem;
            if (getFood > -1)
            {
                getImg.texture = foodType[getFood].foodImage;
            }
            else
            {
                getImg.texture = emptyTexture;
            }
            yield return new WaitForEndOfFrame();
            index++;
        }
        yield return null;
    }
}