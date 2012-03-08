/*  RtwMatrix.cs -- Matrix class.

    Copyright (c) 2004 by Rajitha Wimalasooriya
		Virtusa Corp.
*/

using System;
using System.Text;

namespace rtwmatrix
{
	/// <summary>
	/// Summary description for RtwMatrix.
	/// </summary>
	public class RtwMatrix
	{

		public RtwMatrix(int rows, int columns)
		{
			m_rows = rows;
			m_columns = columns;

			m_matrix = new float[rows,columns];

		}

		private int m_rows;
		private int m_columns;

		private float[,] m_matrix;

		public float this[int row, int column]
		{
			get
			{
				return m_matrix[row,column];
			}
			set
			{
				m_matrix[row,column] = value;
			}
		}

		public int Rows
		{
			get
			{
				return m_rows;
			}
		}

		public int Columns
		{
			get
			{
				return m_columns;
			}
		}

		#region overrides

		public override string ToString()
		{
			int maxSpace = 0;
			
			//find the string length of the longest number in the matrix
			for(int i = 0; i < m_rows; i++)
			{
				for(int j = 0; j < m_columns; j++)
				{
					int currentLen = this[i,j].ToString().Length;

					if(maxSpace < currentLen)
					{
						maxSpace = currentLen;
					}
				}
			}

			//max space needed is one char more than the longest number
			maxSpace++;

			//calculate an approximate value for the string builder length
			StringBuilder sb = new StringBuilder(maxSpace + (m_rows * m_columns));

			for(int i = 0; i < m_rows; i++)
			{
				for(int j = 0; j < m_columns; j++)
				{
					string currentEle = this[i,j].ToString();

					sb.Append(' ', maxSpace - currentEle.Length);
					sb.Append(currentEle);
				}

				sb.Append("\n");
			}

			return sb.ToString();
		}

		public override int GetHashCode()
		{
			float result = 0;

			for(int i = 0; i < m_rows; i++)
			{
				for(int j = 0; j < m_columns; j++)
				{
					result += this[i,j];
				}
			}

			return (int)result;
		}

		public override bool Equals(Object obj)
		{
			RtwMatrix mtx = (RtwMatrix)obj;

			if(this.Rows != mtx.Rows || this.Columns != mtx.Columns)
				return false;

			for(int i = 0; i < this.Rows; i++)
			{
				for(int j = 0; j < this.Columns; j++)
				{
					if(this[i,j] != mtx[i,j])
						return false;
				}
			}

			return true;
		}

		#endregion

		#region operators
 
		//equality
		public static bool operator == (RtwMatrix lmtx, RtwMatrix rmtx)
		{
			return Equals(lmtx, rmtx);
		}

		//inequality
		public static bool operator != (RtwMatrix lmtx, RtwMatrix rmtx)
		{
			return !(lmtx == rmtx);
		}

		//multiplication by matrix
		public static RtwMatrix operator * (RtwMatrix lmtx, RtwMatrix rmtx) 
		{

			if(lmtx.Columns != rmtx.Rows)
				throw new RtwMatrixException("Attempt to multiply matrices with unmatching row and column indexes");
				//return null;

			RtwMatrix result = new RtwMatrix(lmtx.Rows,rmtx.Columns);

			for(int i = 0; i < lmtx.Rows; i++)
			{
				for(int j = 0; j < rmtx.Columns; j++)
				{
					for(int k = 0; k < rmtx.Rows; k++)
					{
						result[i,j] += lmtx[i,k] * rmtx[k,j];
					}
				}
			}

			return result;
		}

		//multiplication by const
		public static RtwMatrix operator * (RtwMatrix mtx, float val)
		{

			RtwMatrix result = new RtwMatrix(mtx.Rows,mtx.Columns);

			for(int i = 0; i < mtx.Rows; i++)
			{
				for(int j = 0; j < mtx.Columns; j++)
				{
					result[i,j] = mtx[i,j] * val;
				}
			}

			return result;
		}

		//multiplication by const
		public static RtwMatrix operator * (float val, RtwMatrix mtx)
		{
			return mtx * val;
		}

        //multiplication by matrix element operation
        public static RtwMatrix operator % (RtwMatrix lmtx, RtwMatrix rmtx)
        {
            if (lmtx.Columns != rmtx.Columns || lmtx.Rows != rmtx.Rows)
                throw new RtwMatrixException("Attempt to element multiply matrices with unmatching row and column indexes");
            //return null;

            RtwMatrix result = new RtwMatrix(lmtx.Rows, lmtx.Columns);

            for (int i = 0; i < lmtx.Rows; i++)
            {
                for (int j = 0; j < lmtx.Columns; j++)
                {
                    result[i, j] = lmtx[i, j] * rmtx[i, j];
                }
            }

            return result;
        }

        //gradiant magnitude
        public static RtwMatrix operator ^ (RtwMatrix lmtx, RtwMatrix rmtx)
        {
            if (lmtx.Columns != rmtx.Columns || lmtx.Rows != rmtx.Rows)
                throw new RtwMatrixException("Attempt to element multiply matrices with unmatching row and column indexes");
            //return null;

            RtwMatrix result = new RtwMatrix(lmtx.Rows, lmtx.Columns);

            for (int i = 0; i < lmtx.Rows; i++)
            {
                for (int j = 0; j < lmtx.Columns; j++)
                {
                    result[i, j] = (float)Math.Sqrt(lmtx[i, j] * lmtx[i,j] + rmtx[i,j] * rmtx[i, j]);
                }
            }

            return result;
        }

        //gradiant orientation (y on the left, x on the right)
        public static RtwMatrix operator & (RtwMatrix lmtx, RtwMatrix rmtx)
        {
            if (lmtx.Columns != rmtx.Columns || lmtx.Rows != rmtx.Rows)
                throw new RtwMatrixException("Attempt to element multiply matrices with unmatching row and column indexes");
            //return null;

            RtwMatrix result = new RtwMatrix(lmtx.Rows, lmtx.Columns);

            for (int i = 0; i < lmtx.Rows; i++)
            {
                for (int j = 0; j < lmtx.Columns; j++)
                {
                    // Tangent = Opposite / Adjacent
                    // Atan2 returns [-PI:PI] in radian
                    result[i, j] = (float)Math.Atan2(lmtx[i, j], rmtx[i, j]);
                    //result[i, j] = (float)Math.Atan2(j-lmtx.Columns/2,i-lmtx.Rows/2);
                }
            }

            return result;
        }

        //devision by const
		public static RtwMatrix operator / (RtwMatrix mtx, float val)
		{

			if(val == 0)
				throw new RtwMatrixException("Attempt to devide the matrix by zero");
				//return null;

			RtwMatrix result = new RtwMatrix(mtx.Rows,mtx.Columns);

			for(int i = 0; i < mtx.Rows; i++)
			{
				for(int j = 0; j < mtx.Columns; j++)
				{
					result[i,j] = mtx[i,j] / val;
				}
			}

			return result;
		}

		//n th power
		public static RtwMatrix operator ^ (RtwMatrix mtx, float val)
		{
			
			if(mtx.Rows != mtx.Columns)
				throw new RtwMatrixException("Attempt to find the power of a non square matrix");
				//return null;

			RtwMatrix result = mtx;

			for(int i = 0; i < val; i++)
			{
				result = result * mtx;
			}

			return result;
		}

		//addition
		public static RtwMatrix operator + (RtwMatrix lmtx, RtwMatrix rmtx)
		{
			if(lmtx.Rows != rmtx.Rows || lmtx.Columns != rmtx.Columns)
				throw new RtwMatrixException("Attempt to add matrixes of different sizes");
				//return null;

			RtwMatrix result = new RtwMatrix(lmtx.Rows,lmtx.Columns);

			for(int i = 0; i < lmtx.Rows; i++)
			{
				for(int j = 0; j < lmtx.Columns; j++)
				{
					result[i,j] = lmtx[i,j] + rmtx[i,j];
				}
			}

			return result;
		}

        //addition with const
        public static RtwMatrix operator + (RtwMatrix lmtx, float val)
        {
            RtwMatrix result = new RtwMatrix(lmtx.Rows, lmtx.Columns);

            for (int i = 0; i < lmtx.Rows; i++)
            {
                for (int j = 0; j < lmtx.Columns; j++)
                {
                    result[i, j] = lmtx[i, j] + val;
                }
            }

            return result;
        }

        //subtraction
		public static RtwMatrix operator - (RtwMatrix lmtx, RtwMatrix rmtx)
		{

			if(lmtx.Rows != rmtx.Rows || lmtx.Columns != rmtx.Columns)
				throw new RtwMatrixException("Attempt to subtract matrixes of different sizes");
				//return null;

			RtwMatrix result = new RtwMatrix(lmtx.Rows,lmtx.Columns);

			for(int i = 0; i < lmtx.Rows; i++)
			{
				for(int j = 0; j < lmtx.Columns; j++)
				{
					result[i,j] = lmtx[i,j] - rmtx[i,j];
				}
			}

			return result;
		}

        //subtraction with const
        public static RtwMatrix operator - (RtwMatrix lmtx, float val)
        {
            return lmtx + (-1 * val);
        }

		//transpose
		public static RtwMatrix operator ~ (RtwMatrix mtx)
		{

			RtwMatrix result = new RtwMatrix(mtx.Columns,mtx.Rows);

			for(int i = 0; i < mtx.Rows; i++)
			{
				for(int j = 0; j < mtx.Columns; j++)
				{
					result[j,i] = mtx[i,j];
				}
			}

			return result;
		}

		//inverse
		public static RtwMatrix operator ! (RtwMatrix mtx)
		{
			
			if(mtx.Determinant() == 0)
				throw new RtwMatrixException("Attempt to invert a singular matrix");
				//return null;

			//inverse of a 2x2 matrix
			if(mtx.Rows == 2 && mtx.Columns == 2)
			{
				RtwMatrix tempMtx = new RtwMatrix(2,2);

				tempMtx[0,0] = mtx[1,1];
				tempMtx[0,1] = -mtx[0,1];
				tempMtx[1,0] = -mtx[1,0];
				tempMtx[1,1] = mtx[0,0];

				return tempMtx / mtx.Determinant();
			}

			return mtx.Adjoint()/mtx.Determinant();

		}

		#endregion

		#region methods

		//determinent
		public float Determinant()
		{

			float determinent = 0;

			if(this.Rows != this.Columns)
				throw new RtwMatrixException("Attempt to find the determinent of a non square matrix");
				//return 0;

			//get the determinent of a 2x2 matrix
			if(this.Rows == 2 && this.Columns == 2)
			{
				determinent = (this[0,0] * this[1,1]) - (this[0,1] * this[1,0]);   
				return determinent;
			}

			RtwMatrix tempMtx = new RtwMatrix(this.Rows - 1, this.Columns - 1);
 
			//find the determinent with respect to the first row
			for(int j = 0; j < this.Columns; j++)
			{
				
				tempMtx = this.Minor(0, j);

				//recursively add the determinents
				determinent += (int)Math.Pow(-1, j) * this[0,j] * tempMtx.Determinant();
				
			}

			return determinent;
		}

		//adjoint matrix
		public RtwMatrix Adjoint()
		{

			if(this.Rows < 2 || this.Columns < 2)
				throw new RtwMatrixException("Adjoint matrix not available");

			RtwMatrix tempMtx = new RtwMatrix(this.Rows-1 , this.Columns-1);
			RtwMatrix adjMtx = new RtwMatrix (this.Columns , this.Rows);

			for(int i = 0; i < this.Rows; i++)
			{
				for(int j = 0; j < this.Columns;j++)
				{

					tempMtx = this.Minor(i, j);

					//put the determinent of the minor in the transposed position
					adjMtx[j,i] = (int)Math.Pow(-1,i+j) * tempMtx.Determinant();
				}
			}

			return adjMtx;
		}

		//returns a minor of a matrix with respect to an element
		public RtwMatrix Minor(int row, int column)
		{

			if(this.Rows < 2 || this.Columns < 2)
				throw new RtwMatrixException("Minor not available");

			int i, j = 0;

			RtwMatrix minorMtx = new RtwMatrix(this.Rows - 1, this.Columns - 1);
 
			//find the minor with respect to the first element
			for(int k = 0; k < minorMtx.Rows; k++)
			{

				if(k >= row)
					i = k + 1;
				else
					i = k;

				for(int l = 0; l < minorMtx.Columns; l++)
				{
					if(l >= column)
						j = l + 1;
					else
						j = l;

					minorMtx[k,l] = this[i,j];
				}

			}

			return minorMtx;
		}

		//returns true if the matrix is identity
		public bool IsIdentity()
		{

			if(Rows != Columns)
				return false;

			for(int i = 0; i < Rows; i++)
			{
				for(int j = 0; j < Columns; j++)
				{
					if(i == j)
					{
						if(this[i,j] != 1.0f) return false;
					}
					else
					{
						if(this[i,j] != 0.0f) return false;
					}
				}
			}

			return true;
		}

		//returns true if the matrix is non singular
		public bool IsInvertible()
		{

			if(this.Determinant() == 0)
			{
				return false;
			}

			return true;
		}

		//makes the matrix an identity matrix
		public void Reset()
		{
			if(m_rows != m_columns)
				throw new RtwMatrixException("Attempt to make non square matrix identity");

			for(int j = 0; j < m_rows; j++)
			{
				for(int i = 0; i < m_columns; i++)
				{
					if(i == j)
					{
						this[i,j] = 1.0f;
					}
					else
					{
						this[i,j] = 0.0f;
					}
				}
			}
		}

		//makes the matrix a zero matrix
		public void Clear()
		{
			for(int j = 0; j < m_rows; j++)
			{
				for(int i = 0; i < m_columns; i++)
				{
					this[i,j] = 0.0f;
				}
			}
		}

        //makes the matrix a one matrix
        public void Ones()
        {
            for (int i = 0; i < m_rows; i++)
            {
                for (int j = 0; j < m_columns; j++)
                {
                    this[i, j] = 1.0f;
                }
            }
        }

        //returns the maximum element value
        public float [] MinMaxValue()
        {
            float min = this[0, 0];
            float max = this[0, 0];
            float min_r = 0;
            float min_c = 0;
            float max_r = 0;
            float max_c = 0;
            for (int i = 0; i < m_rows; i++)
            {
                for (int j = 0; j < m_columns; j++)
                {
                    if (this[i, j] < min)
                    {
                        min = this[i, j];
                        min_r = i;
                        min_c = j;
                    }
                    if (this[i, j] > max)
                    {
                        max = this[i, j];
                        max_r = i;
                        max_c = j;
                    }
                }
            }
            float[] MinMax = { min, max, min_r, min_c, max_r, max_c };            
            return MinMax;
        }

        //clone matrix
        public RtwMatrix Clone()
        {
            RtwMatrix new_matrix = new RtwMatrix(this.Rows, this.Columns);
            for (int i = 0; i < new_matrix.Rows; i++)
            {
                for (int j = 0; j < new_matrix.Columns; j++)
                {
                    new_matrix[i, j] = this[i, j];
                }
            }
            return new_matrix;
        }

        //convert RtwMatrix to 1-D array
        public float[] ToArray()
        {
            int count = 0;
            float[] new_array = new float[this.Rows * this.Columns];
            for (int i = 0; i < this.Rows; i++)
            {
                for (int j = 0; j < this.Columns; j++)
                {
                    new_array[count] = this[i, j];
                    count++;
                }
            }
            return new_array;
        }

        #endregion

    }

    public class RtwMatrixException : Exception
	{
		public RtwMatrixException(string message) : base(message)
		{

		}
	}
}
