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
                node.previous = tail;
            }

            tail = node;
            count++;
        }

        public void Remove(Element<T> node) {
            var current = head;
            var next = node.next;
            var previous = node.previous;

            if (node.next != null) {
                next.previous = node.previous;
            }

            if (node.previous != null) {
                previous.next = node.next;
            }

            node = null;
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

