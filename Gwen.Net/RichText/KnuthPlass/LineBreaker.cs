using System;
using System.Collections.Generic;
using System.Text;

namespace Gwen.Net.RichText.KnuthPlass;
// Knuth and Plass line breaking algorithm
//
// Original JavaScript implementation by Bram Stein
// from https://github.com/bramstein/typeset
// licensed under the new BSD License.
internal class LineBreaker : RichText.LineBreaker {
	public const int Infinity = 10000;

	public const int DemeritsLine = 10;
	public const int DemeritsFlagged = 100;
	public const int DemeritsFitness = 3000;

	private Paragraph? paragraph;
	private int totalWidth;
	private int tolerance;

	private List<Node> nodes = new();

	private Sum sum = new Sum(0, 0, 0);

	private LinkedList<BreakPoint> activeNodes = new LinkedList<BreakPoint>();

	private Formatter formatter;

	public LineBreaker(Renderer.RendererBase renderer, Font defaultFont)
		: base(renderer, defaultFont) {
		formatter = new LeftFormatter(renderer, defaultFont);
	}

	public override List<TextBlock> LineBreak(Paragraph paragraph, int width) {
		List<TextBlock>? textBlocks = null;

		// Todo: Find out why tolerance needs to be quite high sometimes, depending on the line width.
		// Maybe words need to be hyphenated or there is still a bug somewhere in the code.
		for(int tolerance = 4; tolerance < 30; tolerance += 2) {
			textBlocks = DoLineBreak(paragraph, formatter, width, tolerance);
			if(textBlocks != null) {
				break;
			}
		}

		return textBlocks ?? throw new Exception("i dont even know anymore");
	}

	private int GetLineLength(int currentLine) {
		if(paragraph == null) {
			return 0;
		}
		return totalWidth - paragraph.Margin.Left - paragraph.Margin.Right - (currentLine == 1 ? paragraph.FirstIndent : paragraph.RemainigIndent);
	}

	private float ComputeCost(int start, int end, Sum activeTotals, int currentLine) {
		int width = sum.Width - activeTotals.Width;
		int stretch = 0;
		int shrink = 0;

		int lineLength = GetLineLength(currentLine);

		if(nodes[end].Type == NodeType.Penalty) {
			width += nodes[end].Width;
		}

		if(width < lineLength) {
			stretch = sum.Stretch - activeTotals.Stretch;

			if(stretch > 0) {
				return (float)(lineLength - width) / stretch;
			} else {
				return Infinity;
			}
		} else if(width > lineLength) {
			shrink = sum.Shrink - activeTotals.Shrink;

			if(shrink > 0) {
				return (float)(lineLength - width) / shrink;
			} else {
				return Infinity;
			}
		} else {
			return 0.0f;
		}
	}

	private Sum ComputeSum(int breakPointIndex) {
		Sum result = new Sum(sum.Width, sum.Stretch, sum.Shrink);

		for(int i = breakPointIndex; i < nodes.Count; i++) {
			if(nodes[i].Type == NodeType.Glue) {
				result.Width += nodes[i].Width;
				result.Stretch += ((GlueNode)nodes[i]).Stretch;
				result.Shrink += ((GlueNode)nodes[i]).Shrink;
			} else if(nodes[i].Type == NodeType.Box || (nodes[i].Type == NodeType.Penalty && ((PenaltyNode)nodes[i]).Penalty == -Infinity && i > breakPointIndex)) {
				break;
			}
		}

		return result;
	}

	private void MainLoop(int index) {
		Node node = nodes[index];

		LinkedListNode<BreakPoint>? active = activeNodes.First;
		LinkedListNode<BreakPoint>? next = null;
		float ratio = 0.0f;
		int demerits = 0;
		Candidate[] candidates = new Candidate[4];
		int badness;
		int currentLine = 0;
		Sum tmpSum;
		int currentClass = 0;
		int fitnessClass;
		Candidate candidate;
		LinkedListNode<BreakPoint> newNode;

		while(active != null) {
			candidates[0].Demerits = Infinity;
			candidates[1].Demerits = Infinity;
			candidates[2].Demerits = Infinity;
			candidates[3].Demerits = Infinity;

			while(active != null) {
				next = active.Next;
				currentLine = active.Value.Line + 1;
				ratio = ComputeCost(active.Value.Position, index, active.Value.Totals, currentLine);

				if(ratio < -1 || (node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty == -Infinity)) {
					activeNodes.Remove(active);
				}

				if(-1 <= ratio && ratio <= tolerance) {
					badness = (int)(100.0f * Math.Pow(Math.Abs(ratio), 3));

					if(node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty >= 0)
						demerits = (DemeritsLine + badness) * (DemeritsLine + badness) + ((PenaltyNode)node).Penalty * ((PenaltyNode)node).Penalty;
					else if(node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty != -Infinity)
						demerits = (DemeritsLine + badness) * (DemeritsLine + badness) - ((PenaltyNode)node).Penalty * ((PenaltyNode)node).Penalty;
					else
						demerits = (DemeritsLine + badness) * (DemeritsLine + badness);

					if(node.Type == NodeType.Penalty && nodes[active.Value.Position].Type == NodeType.Penalty)
						demerits += DemeritsFlagged * ((PenaltyNode)node).Flagged * ((PenaltyNode)nodes[active.Value.Position]).Flagged;

					if(ratio < -0.5f)
						currentClass = 0;
					else if(ratio <= 0.5f)
						currentClass = 1;
					else if(ratio <= 1.0f)
						currentClass = 2;
					else
						currentClass = 3;

					if(Math.Abs(currentClass - active.Value.FitnessClass) > 1)
						demerits += DemeritsFitness;

					demerits += active.Value.Demerits;

					if(demerits < candidates[currentClass].Demerits) {
						candidates[currentClass].Active = active;
						candidates[currentClass].Demerits = demerits;
						candidates[currentClass].Ratio = ratio;
					}
				}

				active = next;

				if(active != null && active.Value.Line >= currentLine)
					break;
			}

			tmpSum = ComputeSum(index);

			for(fitnessClass = 0; fitnessClass < candidates.Length; fitnessClass++) {
				candidate = candidates[fitnessClass];

				if(candidate.Demerits < Infinity) {
					newNode = new LinkedListNode<BreakPoint>(new BreakPoint(index, candidate.Demerits, candidate.Ratio, candidate.Active.Value.Line + 1, fitnessClass, tmpSum, candidate.Active));
					if(active != null)
						activeNodes.AddBefore(active, newNode);
					else
						activeNodes.AddLast(newNode);
				}
			}
		}
	}

	private List<TextBlock> DoLineBreak(Paragraph paragraph, Formatter formatter, int width, int tolerance) {
		this.paragraph = paragraph;
		totalWidth = width;
		this.tolerance = tolerance;

		nodes = formatter.FormatParagraph(paragraph);

		sum = new Sum(0, 0, 0);

		activeNodes.Clear();
		activeNodes.AddLast(new BreakPoint(0, 0, 0, 0, 0, new Sum(0, 0, 0), null));

		for(int index = 0; index < nodes.Count; index++) {
			Node node = nodes[index];

			if(node.Type == NodeType.Box) {
				sum.Width += node.Width;
			} else if(node.Type == NodeType.Glue) {
				if(index > 0 && nodes[index - 1].Type == NodeType.Box) {
					MainLoop(index);
				}
				sum.Width += node.Width;
				sum.Stretch += ((GlueNode)node).Stretch;
				sum.Shrink += ((GlueNode)node).Shrink;
			} else if(node.Type == NodeType.Penalty && ((PenaltyNode)node).Penalty != Infinity) {
				MainLoop(index);
			}
		}

		if(activeNodes.Count != 0) {
			LinkedListNode<BreakPoint>? node = activeNodes.First;
			LinkedListNode<BreakPoint>? tmp = null;
			while(node != null) {
				if(tmp == null || node.Value.Demerits < tmp.Value.Demerits) {
					tmp = node;
				}

				node = node.Next;
			}

			List<Break> breaks = new List<Break>();

			while(tmp != null) {
				breaks.Add(new Break(tmp.Value.Position, tmp.Value.Ratio));
				tmp = tmp.Value.Previous;
			}

			// breaks.Reverse();

			int lineStart = 0;
			int y = 0;
			int x = 0;
			StringBuilder str = new StringBuilder(1000);
			List<TextBlock> textBlocks = new List<TextBlock>();

			for(int i = breaks.Count - 2; i >= 0; i--) {
				int position = breaks[i].Position;
				float r = breaks[i].Ratio;

				for(int j = lineStart; j < nodes.Count; j++) {
					if(nodes[j].Type == NodeType.Box || (nodes[j].Type == NodeType.Penalty && ((PenaltyNode)nodes[j]).Penalty == -Infinity)) {
						lineStart = j;
						break;
					}
				}

				int height = 0;
				int baseline = 0;
				for(int nodeIndex = lineStart; nodeIndex <= position; nodeIndex++) {
					if(nodes[nodeIndex].Type == NodeType.Box) {
						height = Math.Max(height, ((BoxNode)nodes[nodeIndex]).Height);
						Font font = ((TextPart)((BoxNode)nodes[nodeIndex]).Part).Font ?? DefaultFont;
						baseline = Math.Max(baseline, (int)font.FontMetrics.Baseline);
					}
				}

				Part part = ((BoxNode)nodes[lineStart]).Part;
				int blockStart = lineStart;
				for(int nodeIndex = lineStart; nodeIndex <= position; nodeIndex++) {
					if((nodes[nodeIndex].Type == NodeType.Box && ((BoxNode)nodes[nodeIndex]).Part != part) || nodeIndex == position) {
						TextBlock textBlock = new TextBlock();
						textBlock.Part = part;
						str.Clear();

						for(int k = blockStart; k < (nodeIndex - 1); k++) {
							if(nodes[k].Type == NodeType.Glue) {
								if(nodes[k].Width > 0)
									str.Append(' ');
							} else if(nodes[k].Type == NodeType.Box) {
								str.Append(((BoxNode)nodes[k]).Value);
							}
						}

						{
							Font font = ((TextPart)part).Font ?? DefaultFont;

							textBlock.Position = new Point(x, y + baseline - (int)font.FontMetrics.Baseline);
							textBlock.Text = str.ToString();
							textBlock.Size = new Size(formatter.MeasureText(font, textBlock.Text).Width, height);
						}

						x += textBlock.Size.Width;

						textBlocks.Add(textBlock);

						if(nodes[nodeIndex].Type == NodeType.Box)
							part = ((BoxNode)nodes[nodeIndex]).Part;
						blockStart = nodeIndex;
					}
				}

				x = 0;
				y += height;

				lineStart = position;
			}

			return textBlocks;
		}

		return new List<TextBlock>();
	}

	private struct Candidate {
		public LinkedListNode<BreakPoint> Active { get; set; }
		public int Demerits { get; set; }
		public float Ratio { get; set; }

#if DEBUG
		public override string ToString() {
			return String.Format("Candidate: Demerits = {0} Ratio = {1} Active = {2}", Demerits, Ratio, Active.Value.ToString());
		}
#endif
	}

	private struct Break {
		public int Position { get; set; }
		public float Ratio { get; set; }

		public Break(int position, float ratio) {
			Position = position;
			Ratio = ratio;
		}
	}

	private struct Sum {
		public int Width { get; set; }
		public int Stretch { get; set; }
		public int Shrink { get; set; }

		public Sum(int width, int stretch, int shrink) {
			Width = width;
			Stretch = stretch;
			Shrink = shrink;
		}

#if DEBUG
		public override string ToString() {
			return String.Format("Sum: Width = {0} Stretch = {1} Shrink = {2}", Width, Stretch, Shrink);
		}
#endif
	}

	private struct BreakPoint {
		public int Position { get; set; }
		public int Demerits { get; set; }
		public float Ratio { get; set; }
		public int Line { get; set; }
		public int FitnessClass { get; set; }
		public Sum Totals { get; set; }
		public LinkedListNode<BreakPoint>? Previous { get; set; }

		public BreakPoint(int position, int demerits, float ratio, int line, int fitnessClass, Sum totals, LinkedListNode<BreakPoint>? previous) {
			Position = position;
			Demerits = demerits;
			Ratio = ratio;
			Line = line;
			FitnessClass = fitnessClass;
			Totals = totals;
			Previous = previous;
		}

#if DEBUG
		public override string ToString() {
			return String.Format("BreakPoint: Position = {0} Demerits = {1} Ratio = {2} Line = {3} FitnessClass = {4} Totals = {{{5}}} Previous = {{{6}}}", Position, Demerits, Ratio, Line, FitnessClass, Totals.ToString(), Previous != null ? Previous.Value.ToString() : "Null");
		}
#endif
	}
}