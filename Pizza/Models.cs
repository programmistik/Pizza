using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCodePizza
{
    public enum Ingredients
    {
        Tomato, Mushroom
    }
    public class MyPizza
    {
        enum SliceStatus
        {
            ValidSlice, TooLittleSlice, TooBigSlice, InvalidSlice
        }

        private int Columns;
        private int Rows;

        private int MinIngPerSlice;
        private int MaxSliceSize;

        private int[,] Pizza;

        public MyPizza(int Rows, int Columns, int[,] Pizza, int MinIngPerSlice, int MaxSliceSize)
        {
            this.Rows = Rows;
            this.Columns = Columns;
            this.Pizza = Pizza;
            this.MinIngPerSlice = MinIngPerSlice;
            this.MaxSliceSize = MaxSliceSize;
        }
        
        public int GetSize() => Columns * Rows; 

        public List<Slice> CutPizza()
        {
            int[,] pizza = Pizza.Clone() as int[,];

            // create slice hash
            var nextSliceId = -1;
            var sliceHash = new Dictionary<int, Slice>();

            // Cut pizza
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (CutPizzaAtPosition(pizza, r, c, sliceHash, nextSliceId) == true)
                        nextSliceId--;
                }
            }

            // Try cut one more
            var slices = new List<Slice>(sliceHash.Values);
            foreach (var slice in slices)
            {
                var currentSlice = sliceHash[slice.Id];

                sliceHash.Remove(currentSlice.Id);
                currentSlice.RestoreSlice(pizza, Pizza);

                CutPizzaAtPosition(pizza, currentSlice.MinRow, currentSlice.MinCol, sliceHash, currentSlice.Id);
            }

            return new List<Slice>(sliceHash.Values);
        }

       
        private bool CutPizzaAtPosition(int[,] pizza, int r, int c, Dictionary<int, Slice> sliceHash, int nextSliceId)
        {
            if (pizza[r, c] < 0)
                return false;

            var maxSlice = GetMaxSliceExtentionAt(pizza, sliceHash, r, c, nextSliceId);
            if (maxSlice != null)
            {
                var SliceContent = maxSlice.GetSliceContent(pizza);
                foreach (var OverlapSliceId in SliceContent.Keys)
                {
                    if (OverlapSliceId > 0)
                        continue;

                    var existingSlice = sliceHash[OverlapSliceId];
                    var existingAfterOverlap = existingSlice.CreateOverlapSlice(maxSlice);
                    sliceHash[existingSlice.Id] = existingAfterOverlap;
                }

                maxSlice.RemoveSlice(pizza);
                sliceHash.Add(maxSlice.Id, maxSlice);

                return true;
            }

            return false;
        }

        private Slice GetMaxSliceExtentionAt(int[,] pizza, Dictionary<int, Slice> sliceHash, int row, int column, int nextSliceId)
        {
            Slice maxSlice = null;
            int maxSliceIng = 0;

            for (int minRow = row; minRow >= Math.Max(0, row - MaxSliceSize); minRow--)
                for (int maxRow = row; maxRow < Math.Min(row + MaxSliceSize + 1, Rows); maxRow++)
                {
                    for (int minCol = column; minCol >= Math.Max(0, column - MaxSliceSize); minCol--)
                        for (int maxCol = column; maxCol < Math.Min(column + MaxSliceSize + 1, Columns); maxCol++)
                        {
                            var isValidSlice = IsValidSlice(Pizza, minRow, maxRow, minCol, maxCol);
                            if ((isValidSlice == SliceStatus.TooBigSlice) || (isValidSlice == SliceStatus.InvalidSlice))
                                break;

                            if (isValidSlice != SliceStatus.ValidSlice)
                                continue;

                            var newSlice = new Slice(nextSliceId, minRow, maxRow, minCol, maxCol);

                            var newSliceIng = newSlice.CountIngredients(pizza);
                            if (newSliceIng == 0)
                                continue;

                            var SliceContent = newSlice.GetSliceContent(pizza);
                            var isValidOverlap = true;
                            foreach (var OverlapSliceId in SliceContent.Keys)
                            {
                                if (OverlapSliceId > 0)
                                    continue;

                                var existingSlice = sliceHash[OverlapSliceId];
                                var existingAfterOverlap = existingSlice.CreateOverlapSlice(newSlice);
                                if (existingAfterOverlap == null)
                                {
                                    isValidOverlap = false;
                                    break;
                                }

                                if (IsValidSlice(Pizza,
                                    existingAfterOverlap.MinRow, existingAfterOverlap.MaxRow,
                                    existingAfterOverlap.MinCol, existingAfterOverlap.MaxCol) != SliceStatus.ValidSlice)
                                {
                                    isValidOverlap = false;
                                    break;
                                }
                            }
                            if (isValidOverlap == false)
                                continue;

                            // Check if the new slice is bettter than existing max
                            if (maxSlice == null)
                            {
                                maxSlice = newSlice;
                                maxSliceIng = newSliceIng;
                            }
                            else if (maxSliceIng < newSliceIng)
                            {
                                maxSlice = newSlice;
                                maxSliceIng = newSliceIng;
                            }
                        }
                }

            return maxSlice;
        }

        public bool IsValidSlicing(List<Slice> slices)
        {
            int[,] pizza = Pizza.Clone() as int[,];
            foreach (Slice slice in slices)
            {
                if (IsValidSlice(Pizza, slice.MinRow, slice.MaxRow, slice.MinCol, slice.MaxCol) != SliceStatus.ValidSlice)
                    return false;

                for (int r = slice.MinRow; r <= slice.MaxRow; r++)
                    for (int c = slice.MinCol; c <= slice.MaxCol; c++)
                    {
                        if (pizza[r, c] < 0)
                            return false;
                        pizza[r, c] = slice.Id;
                    }
            }

            return true;
        }

        private SliceStatus IsValidSlice(int[,] pizza, int minRow, int maxRow, int minCol, int maxCol)
        {
            var TomatoCount = 0;
            var MushroomCount = 0;

            if ((maxRow - minRow + 1) * (maxCol - minCol + 1) > MaxSliceSize)
                return SliceStatus.TooBigSlice;

            for (int r = minRow; r <= maxRow; r++)
            {
                for (int c = minCol; c <= maxCol; c++)
                {
                    var Value = pizza[r, c];
                    if (Value <= 0)
                        return SliceStatus.InvalidSlice;
                    else if (Value == 1) //Tomato
                        TomatoCount++;
                    else if (Value == 2) //Mushroom
                        MushroomCount++;
                }
            }

            if ((TomatoCount < MinIngPerSlice) || (MushroomCount < MinIngPerSlice))
                return SliceStatus.TooLittleSlice;

            return SliceStatus.ValidSlice;
        }
    }
    

    public class Slice
    {
        public int Id { get; private set; }
        public int MinRow { get; private set; }
        public int MaxRow { get; private set; }
        public int MinCol { get; private set; }
        public int MaxCol { get; private set; }

        public Slice(int Id, int MinRow, int MaxRow, int MinCol, int MaxCol)
        {
            this.Id = Id;
            this.MinRow = MinRow;
            this.MaxRow = MaxRow;
            this.MinCol = MinCol;
            this.MaxCol = MaxCol;
        }

        public int GetSize() => (MaxRow - MinRow + 1) * (MaxCol - MinCol + 1);
      

        public int CountIngredients(int[,] pizza)
        {
            var count = 0;

            for (int r = MinRow; r <= MaxRow; r++)
                for (int c = MinCol; c <= MaxCol; c++)
                    if (pizza[r, c] > 0)
                        count++;

            return count;
        }

        public Dictionary<int, int> GetSliceContent(int[,] pizza)
        {
            var ingredients = new Dictionary<int, int>();
            for (int r = MinRow; r <= MaxRow; r++)
                for (int c = MinCol; c <= MaxCol; c++)
                {
                    var key = pizza[r, c];
                    int value;

                    if (ingredients.TryGetValue(key, out value) == false)
                        value = 0;

                    ingredients[key] = value++;
                }

            return ingredients;
        }

        public void RemoveSlice(int[,] pizza)
        {
            for (int r = MinRow; r <= MaxRow; r++)
                for (int c = MinCol; c <= MaxCol; c++)
                    pizza[r, c] = Id;
        }

        public void RestoreSlice(int[,] pizza, int[,] sourcePizza)
        {
            for (int r = MinRow; r <= MaxRow; r++)
                for (int c = MinCol; c <= MaxCol; c++)
                    pizza[r, c] = sourcePizza[r, c];
        }

        public static int GetSlicesSize(ICollection<Slice> slices)
        {
            var size = 0;
            foreach (Slice slice in slices)
                size += slice.GetSize();

            return size;
        }

        public Slice CreateOverlapSlice(Slice newSlice)
        {
            if ( ((newSlice.MinCol > MinCol) || (newSlice.MaxCol < MaxCol)) &&
                ((newSlice.MinRow > MinRow) || (newSlice.MaxRow < MaxRow)) )
                return null;

            var newMinRow = MinRow;
            var newMaxRow = MaxRow;
            var newMinCol = MinCol;
            var newMaxCol = MaxCol;

            if ((newSlice.MinCol <= MinCol) && (newSlice.MaxCol >= MaxCol))
            {
                if ((newSlice.MinRow > MinRow) && (newSlice.MaxRow < MaxRow)) // Middle
                    return null;
                
                if (newSlice.MinRow <= MinRow) // Above
                    newMinRow = newSlice.MaxRow + 1;
                
                if (newSlice.MaxRow >= MaxRow) // Below
                    newMaxRow = newSlice.MaxRow - 1;
            }

            if ((newSlice.MinRow <= MinRow) && (newSlice.MaxRow >= MaxRow))
            {
                if ((newSlice.MinCol > MinCol) && (newSlice.MaxCol < MaxCol)) // Middle
                    return null;

                if (newSlice.MinCol <= MinCol) // Left
                    newMinCol = newSlice.MaxCol + 1;
                
                if (newSlice.MaxCol >= MaxCol) // Right
                    newMaxCol = newSlice.MinCol - 1;
            }

            if (newMinCol > newMaxCol)
                return null;
            if (newMinRow > newMaxRow)
                return null;

            return new Slice(Id, newMinRow, newMaxRow, newMinCol, newMaxCol);
        }
    }
}

