namespace collections {
    public class Element<T>{
        public T value;
        public Element<T> previous;
        public Element<T> next;

        public static Element<T> SetValue(T value) {
            return new Element<T> {
                value = value
            };
        }
    }

    public class BindableList<T> {
        public Element<T> start;
        public Element<T> end;
        public int count;

        public void Add(T value) {
            var elem = Element<T>.SetValue(value);

            if (start == null) {
                start = elem;
            } else {
                end.next = elem;
                elem.previous = end;
            }

            end = elem;
            count++;
        }

        public void Remove(T value) {
            var current = start;

            while (current != null) {
                if (current.value.Equals(value)) {
                    break;
                }

                current = current.next;
            }

            if (current != null) {
                if (current.next != null) {
                    current.next.previous = current.previous;
                } else {
                    end = current.previous;
                }
 
                if (current.previous != null) {
                    current.previous.next = current.next;
                } else {
                    start = current.next;
                }
                count--;
            }
        }

        public void Clear() {
            start = null;
            end = null;
            count = 0;
        }
    }
}

