using System;

namespace Plugins.LeapMotion {
    /// <summary>
    /// Circular LIFO Buffer
    /// </summary>
    public class CircularBuffer<T> {
        T[] _buffer;
        
        // define the current Frame to fill, so the previous one is the last filled and the real current
        int _currentToFill = 0;
        
        public CircularBuffer(int size) {
            _buffer = new T[size];
        }

        public void Put(T o) {
            _buffer[_currentToFill] = o;
            _currentToFill = WrapIndex(_currentToFill + 1);
        }
        
        /// <returns>Return the last filled value</returns>
        public T Get() {
            return History(0);
        }

        public T History(int history) {
            if (history < 0 || history >= _buffer.Length) throw new ArgumentException();
            return _buffer[WrapIndex(_currentToFill - 1 - history)];
        }

        int WrapIndex(int index) {
            if (index <= -_buffer.Length) throw new NotSupportedException();
            if (index >= 0) return index % _buffer.Length;
            return index + _buffer.Length;
        }
    }

}