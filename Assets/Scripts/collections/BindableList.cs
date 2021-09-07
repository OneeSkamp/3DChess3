using System.Collections.Generic;
using System.Collections;

namespace collections {
    public class Element<T> {
        public T value;

        public Element<T> previous;
        public Element<T> next;

        public static Element<T> Mk(T value) {
            return new Element<T> {
                value = value
            };
        }
    }

    public class BindableList<T> : IEnumerable<Element<T>> {
        public Element<T> head;
        public Element<T> tail;

        public int count;

    public BindableList() {
        head = new Element<T>();
        tail = new Element<T>();

        head.next = tail;
        tail.previous = head;
    }

        public void Add(T value) {
            var node = Element<T>.Mk(value);

            tail.previous.next = node;
            node.previous = tail.previous;
            node.next = tail;
            tail.previous = node;

            count++;
        }

        public void Remove(Element<T> node) {
            var current = node;

            current.next.previous = current.previous;
            current.previous.next = current.next;

            count--;
        }

        public void Clear() {
            head = null;
            tail = null;
            count = 0;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<Element<T>> IEnumerable<Element<T>>.GetEnumerator() {
            var node = head.next;
            if (node != null) {
                while (node != tail.previous) {
                    yield return node;

                    node = node.next;
                }

                yield return node;
            }
        }
    }
}