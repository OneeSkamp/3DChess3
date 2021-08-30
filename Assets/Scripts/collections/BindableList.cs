using System.Collections.Generic;
using System.Collections;
using UnityEngine;

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

    public class BindableList<T> : IEnumerable<Element<T>>{
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

            if (count == 0) {
                head = node;
                tail = head;
            } else {
                tail.next = node;
                node.next = head;
                node.previous = tail;
            }

            tail = node;
            count++;
        }

        public void Remove( Element<T> node) {
            var current = node;
            Debug.Log(node.value);
            if (current.next == null && current.previous != null) {
                current.previous.next = null;
            }

            if (current.next != null && current.previous == null) {
                Debug.Log("+");
                current.next.previous = null;
            }

            if (current.next != null && current.previous != null) {
                Debug.Log("+");
                current.next.previous = current.previous;
                current.previous.next = current.next;
            }

            if (current.next == null && current.previous == null) {
                current = null;
            }

            if (current.previous != null) {
                current.previous.next = current.next;
                //previous.next = node.next;
            }
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
            var node = head;
            while (node != null) {
                yield return node;

                node = node.next;
            }
        }
    }
}

