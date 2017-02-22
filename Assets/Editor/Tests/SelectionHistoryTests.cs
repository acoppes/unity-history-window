using UnityEngine;
using NUnit.Framework;

namespace Gemserk
{
	public class SelectionHistoryTests {

		[Test]
		public void NextAndPreviousShouldntExplodeWithoutItems()
		{
			var selectionHistory = new SelectionHistory();

			selectionHistory.Previous();
			selectionHistory.Next();

			Assert.Pass();
		}


		[Test]
		public void NavigationWindowTest()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();

			selectionHistory.UpdateSelection(selection1);

			Assert.AreSame(selectionHistory.GetSelection(), selection1);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 1);
		}

		[Test]
		public void NavigationWindowTest2()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);

			Assert.AreSame(selectionHistory.GetSelection(), selection2);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);
		}

		[Test]
		public void UpdateWithSameSelectionShouldntAddTwiceToHistory()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection1);

			Assert.AreSame(selectionHistory.GetSelection(), selection1);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 1);
		}

		[Test]
		public void TestPreviousSelectionShouldntStoreInHistory()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);

			Assert.AreSame(selectionHistory.GetSelection(), selection2);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);

			selectionHistory.Previous();

			Assert.AreSame(selectionHistory.GetSelection(), selection1);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);

			selectionHistory.Next();

			Assert.AreSame(selectionHistory.GetSelection(), selection2);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);
		}

		[Test]
		public void TestPreviousAndNextShouldntUpdateHistory()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);

			Assert.AreSame(selectionHistory.GetSelection(), selection2);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);

			selectionHistory.Previous();
			selectionHistory.UpdateSelection(selection1);

			Assert.AreSame(selectionHistory.GetSelection(), selection1);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);

			selectionHistory.Next();
			selectionHistory.UpdateSelection(selection2);

			Assert.AreSame(selectionHistory.GetSelection(), selection2);
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);
		}

		[Test]
		public void ClearDeletedEntriesShouldKeepSelectedIndex()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();
			var selection3 = new GameObject();
			var selection4 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);
			selectionHistory.UpdateSelection(selection3);
			selectionHistory.UpdateSelection(selection4);

			Assert.IsTrue(selectionHistory.IsSelected(3));
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 4);

			GameObject.DestroyImmediate (selection2);

			selectionHistory.ClearDeleted ();

			Assert.IsTrue(selectionHistory.IsSelected(2));
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 3);

			selectionHistory.UpdateSelection (1);

			GameObject.DestroyImmediate (selection1);
			GameObject.DestroyImmediate (selection4);

			selectionHistory.ClearDeleted ();

			Assert.IsTrue(selectionHistory.IsSelected(0));
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 1);
		}

		[Test]
		public void ClearDeletedItemsShouldntSelectIfSelectionDeleted()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();
			var selection3 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);
			selectionHistory.UpdateSelection(selection3);

			Assert.IsTrue(selectionHistory.IsSelected(2));
			Assert.That(selectionHistory.GetHistoryCount(), Is.EqualTo(3));

			GameObject.DestroyImmediate (selection3);

			selectionHistory.ClearDeleted ();

			Assert.IsFalse(selectionHistory.IsSelected(0));
			Assert.IsFalse(selectionHistory.IsSelected(1));
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 2);
		}

		[Test]
		public void TestRemoveDuplicatedElementsInOrder()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = new GameObject();
			var selection2 = new GameObject();
			var selection3 = new GameObject();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);
			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection3);
			selectionHistory.UpdateSelection(selection1);

			selectionHistory.RemoveDuplicated();

			Assert.That(selectionHistory.GetHistoryCount(), Is.EqualTo(3));
			Assert.That(selectionHistory.GetSelection(), Is.SameAs(selection1)); 
			Assert.That(selectionHistory.IsSelected(2), Is.True);

			selectionHistory.UpdateSelection (1);
			Assert.That(selectionHistory.GetSelection(), Is.SameAs(selection3)); 

			selectionHistory.UpdateSelection (0);
			Assert.That(selectionHistory.GetSelection(), Is.SameAs(selection2)); 
		}

	}
}