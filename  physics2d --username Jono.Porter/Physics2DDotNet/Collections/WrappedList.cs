#region MIT License
/*
 * Copyright (c) 2005-2007 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion



using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
namespace Physics2DDotNet.Collections
{
    public class WrappedList<T, TList> : WrappedCollection<T, TList>, IList<T>
    where TList : IList<T>
    {
        public WrappedList(TList self) : base(self) { }
        public WrappedList(TList self, AdvReaderWriterLock selfLock) : base(self, selfLock) { }

        public T this[int index]
        {
            get
            {
                using (Lock.Read)
                {
                    return This[index];
                }
            }
            set
            {
                using (Lock.Write)
                {
                    This[index] = value;
                }
            }
        }

        public int IndexOf(T item)
        {
            using (Lock.Read)
            {
                return This.IndexOf(item);
            }
        }
        public void Insert(int index, T item)
        {
            using (Lock.Write)
            {
                This.Insert(index, item);
            }
        }
        public void RemoveAt(int index)
        {
            using (Lock.Write)
            {
                This.RemoveAt(index);
            }
        }
    }
    public class ListWrapper<T> : WrappedList<T, IList<T>>
    {
        public ListWrapper(IList<T> self) : base(self) { }
        public ListWrapper(IList<T> self, AdvReaderWriterLock selfLock) : base(self, selfLock) { }
    }
}