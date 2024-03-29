﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ToxicRagers.Compression.Huffman
{
    public class Tree
    {
        List<Node> nodes = new List<Node>();
        public string Json { get; set; }
        Node root;

        FrequencyTable frequencies = new FrequencyTable();

        int bitCount = 0;
        int leafCount = 0;

        public FrequencyTable FrequencyTable
        {
            get => frequencies;
            set => frequencies = value;
        }

        public int LeafCount => (leafCount * 2) + 1;

        public void BuildTree(byte[] source = null)
        {
            nodes.Clear();

            if (source != null) { frequencies.Import(source); }

            foreach (KeyValuePair<byte, int> symbol in frequencies.Frequencies)
            {
                nodes.Add(new Node { Symbol = symbol.Key, Frequency = symbol.Value });
            }

            while (nodes.Count > 1)
            {
                List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ThenBy(node => node.Symbol).ToList();

                if (orderedNodes.Count >= 2)
                {
                    List<Node> takenNodes = orderedNodes.Take(2).ToList();

                    Node parent = new Node
                    {
                        Frequency = takenNodes[0].Frequency + takenNodes[1].Frequency,
                        Left = takenNodes[0],
                        Right = takenNodes[1]
                    };

                    nodes.Remove(takenNodes[0]);
                    nodes.Remove(takenNodes[1]);
                    nodes.Add(parent);

                    leafCount++;
                }
            }

            root = nodes.FirstOrDefault();

            Json = JsonConvert.SerializeObject(root, Formatting.Indented);
        }

        public void FromByteArray(byte[] array, int leafCount)
        {
            List<bool> decodedDictionary = new List<bool>();

            for (int j = 0; j < array.Length; j++)
            {
                byte b = array[j];

                for (int i = 0; i < 8; i++)
                {
                    decodedDictionary.Add((b & (0x80 >> i)) > 0);
                }
            }

            int offset = 0;
            int offset2 = 1;

            Node node = new Node();
            Node parentNode = new Node();
            Stack<Node> nodeStack = new Stack<Node>();

            // The tree is encoded as a big byte array using 2 it boundaries to denote:
            //     1 0 = new branch in the tree
            //     1 1 = start of leaf, start of data
            //     0 0 = end of leaf, end of data, step up
            // Data is 8 bits making up the byte value of the colour component
            // After processing the left node it will process the right node
            //                               11 ... 00
            //                         ┌─────┬──────┐
            //                  10     │     │3     │
            //                 ┌───────┤     └──────┘
            //          ┌──────┤2      │     11 ... 00
            //          │      └───────┤     ┌──────┐
            // 10       │              │     │4     │
            // ┌────────┤              └─────┴──────┘
            // │        │
            // │ 1      │                    11 ... 00
            // └────────┤              ┌─────┬──────┐
            //          │       10     │     │ 6    │
            //          │      ┌───────┤     └──────┘
            //          └──────┤ 5     │      11 ... 00
            //                 └───────┤     ┌──────┐
            //                         │     │ 7    │
            //                         └─────┴──────┘
            //  Output: 10 10 11...00 11...00 10 11...00 11...00
            //
            while (true)
            {
	            if (offset < decodedDictionary.Count)
	            {
		            if (decodedDictionary[offset] && !decodedDictionary[offset + 1])
		            {
			            parentNode = new Node();
			            nodeStack.Push(parentNode);
			            nodes.Add(parentNode);
		            }

		            else if (decodedDictionary[offset] && decodedDictionary[offset + 1])
		            {
			            offset += 2;
			            node = new Node();
			            nodes.Add(node);
			            for (int i = 0; i < 8; i++)
			            {
				            node.Symbol |= (byte)(decodedDictionary[offset++] ? (0x80 >> i) : 0);
			            }


			            if (decodedDictionary[offset] || decodedDictionary[offset + 1])
			            {
				            throw new InvalidDataException(
					            $"Expecting the next two bits in Huffman tree to be 00! Got: {(decodedDictionary[offset] ? "1" : "0")}{(decodedDictionary[offset + 1] ? "1" : "0")} at offsets {offset} and {offset + 1}");
			            }

			            if (parentNode.Left == null)
			            {
				            parentNode.Left = node;
			            }
			            else if (parentNode.Right == null)
			            {
				            parentNode.Right = node;
			            }

			            while (nodeStack.Count > 0 && parentNode.Left != null && parentNode.Right != null)
			            {
				            nodeStack.Pop();
				            node = parentNode;
				            if (nodeStack.Count > 0)
				            {
					            parentNode = nodeStack.Peek();

					            if (parentNode.Left == null)
					            {
						            parentNode.Left = node;
					            }
					            else if (parentNode.Right == null)
					            {
						            parentNode.Right = node;
					            }
				            }
			            }
		            }

		            offset += 2;
	            }
	            else
	            {
		            if (nodeStack.Count > 0)
		            {
			            if (parentNode.Left == null)
			            {
				            parentNode.Left = node;
			            }
			            else if (parentNode.Right == null)
			            {
				            parentNode.Right = node;
			            }
                        node = parentNode;
                        nodeStack.Pop();
                        if (nodeStack.Count > 0)
                        {
	                        parentNode = nodeStack.Peek();
                        }

                    }
		            else
		            {
			            break;
		            }
	            }
            }

            while (nodeStack.Count > 0)
            {
	            node = parentNode;
	            parentNode = nodeStack.Pop();
                if (parentNode.Left == null)
	            {
		            parentNode.Left = node;
	            }
	            else if (parentNode.Right == null)
	            {
		            parentNode.Right = node;
	            }
            }

            root = nodes.FirstOrDefault();

            Json = JsonConvert.SerializeObject(root, Formatting.Indented);
        }

        public byte[] ToByteArray()
        {
            List<bool> encodedDictionary = new List<bool>();
            processNode(root, encodedDictionary);
            return boolListToByteArray(encodedDictionary);
        }
        
        private void processNode(Node node, List<bool> data)
        {
            if (node.Left == null && node.Right == null)
            {
                if (data.Count == 0) { data.Add(true); data.Add(false); }
                data.Add(true); data.Add(true);

                byte mask = 0x80;

                for (int i = 0; i < 8; i++)
                {
                    data.Add((node.Symbol & mask) == mask);
                    mask >>= 1;
                }

                data.Add(false); data.Add(false);
            }
            else
            {
                data.Add(true); data.Add(false);

                if (node.Left != null) { processNode(node.Left, data); }
                if (node.Right != null) { processNode(node.Right, data); }
            }
        }

        public byte[] Encode(byte[] source)
        {
            List<bool> encodedSource = new List<bool>();

            Dictionary<byte, List<bool>> tree = new Dictionary<byte, List<bool>>();

            for (int i = 0; i < 256; i++)
            {
                List<bool> list = root.Traverse((byte)i, new List<bool>());
                if (list != null) { tree.Add((byte)i, list); }
            }

            encodedSource.AddRange(
                source.SelectMany(
                    character => tree[character]
                ).ToList()
            );

            bitCount = encodedSource.Count;

            return boolListToByteArray(encodedSource);
        }

        public byte[] Decode(byte[] source)
        {
	        List<bool> encodedSource = byteArrayToBoolList(source);
	        List<byte> output = new List<byte>();
            for(int i = 0; i < encodedSource.Count; i++)
            {
	            Node currentNode = root;
	            while (i < encodedSource.Count && (currentNode.Left != null || currentNode.Right != null))
	            {
		            if (encodedSource[i] == false)
		            {
			            if (currentNode.Left == null)
			            {
				            throw new InvalidDataException(
					            $"Bit at position {i} says to go left by the left node is null!");
			            }
			            currentNode = currentNode.Left;
		            }
		            else
		            {
			            if (currentNode.Right == null)
			            {
				            throw new InvalidDataException(
					            $"Bit at position {i} says to go right by the right node is null!");
			            }
                        currentNode = currentNode.Right;
		            }

		            i++;
	            }

	            i--;
                output.Add(currentNode.Symbol);
            }

            return output.ToArray();
        }

        private static byte[] boolListToByteArray(List<bool> list)
        {
            byte[] bytes = new byte[list.Count / 8 + (list.Count % 8 == 0 ? 0 : 1)];

            int b = 0;
            int j = 7;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i]) { bytes[b] |= (byte)(1 << j); }

                j--;

                if (j < 0)
                {
                    j = 7;
                    b++;
                }
            }

            return bytes;
        }
        private static List<bool> byteArrayToBoolList(byte[] bytes)
        {
	        List<bool> list = new List<bool>();
            //byte[] bytes = new byte[list.Count / 8 + (list.Count % 8 == 0 ? 0 : 1)];

            int b = 0;
            int j = 7;

            for (int i = 0; i < bytes.Length; i++)
            {
	            for (int k = 7; k >= 0; k--)
	            {
					list.Add(((bytes[i] >> k) & 1)  == 1);
	            }
            }

            return list;
        }
    }
}