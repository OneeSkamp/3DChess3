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
        public Element<T> root;
        public int count;

        public void Add(T value) {
            var node = Element<T>.Mk(value);

            if (count == 0) {
                root = node;
                root.previous = root;
            } else {
                root.previous.next = node;
                node.previous = root.previous;
                root.previous = node;
                node.next = root;
            }
            count++;
        }

        public void Remove(Element<T> node) {
            var current = node;

            if (count != 1) {
                if (current == root) {
                    root = current.next;
                }

                current.next.previous = current.previous;
                current.previous.next = current.next;
            } else {
                root = null;
            }
            count--;
        }

        public void Clear() {
            root = null;
            count = 0;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)this).GetEnumerator();
        }

        IEnumerator<Element<T>> IEnumerable<Element<T>>.GetEnumerator() {
            var node = root;

            while (node != root.previous) {
                yield return node;

                node = node.next;
            }

            yield return node;
        }
    }
}

