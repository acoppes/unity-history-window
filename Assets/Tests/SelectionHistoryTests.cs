using UnityEngine;
using NUnit.Framework;

namespace Gemserk
{
	public class AssetForTests : ScriptableObject
	{
		
	}
	
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
		public void ClearDeletedEntries_ShouldKeepSelectedIndex_IfNotDeleted()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = ScriptableObject.CreateInstance<AssetForTests>();
			var selection2 = ScriptableObject.CreateInstance<AssetForTests>();
			var selection3 = ScriptableObject.CreateInstance<AssetForTests>();
			var selection4 = ScriptableObject.CreateInstance<AssetForTests>();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);
			selectionHistory.UpdateSelection(selection3);
			selectionHistory.UpdateSelection(selection4);

			Assert.AreEqual(3, selectionHistory.GetSelectedIndex());
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 4);

			Object.DestroyImmediate (selection2);

			selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

			Assert.AreEqual(2, selectionHistory.GetSelectedIndex());
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 3);

			selectionHistory.SetSelection (selection3);

			Object.DestroyImmediate (selection1);
			Object.DestroyImmediate (selection4);

			selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

			Assert.IsTrue(selectionHistory.IsSelected(0));
			Assert.AreEqual(selectionHistory.GetHistoryCount(), 1);
		}

		[Test]
		public void ClearDeletedEntries_ShouldNotKeepSelectedIndex_IfDeleted()
		{
			var selectionHistory = new SelectionHistory();

			var selection1 = ScriptableObject.CreateInstance<AssetForTests>();
			var selection2 = ScriptableObject.CreateInstance<AssetForTests>();
			var selection3 = ScriptableObject.CreateInstance<AssetForTests>();

			selectionHistory.UpdateSelection(selection1);
			selectionHistory.UpdateSelection(selection2);
			selectionHistory.UpdateSelection(selection3);

			Assert.AreEqual(2, selectionHistory.GetSelectedIndex());
			Assert.That(selectionHistory.GetHistoryCount(), Is.EqualTo(3));

			Object.DestroyImmediate(selection3);

			selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

			Assert.IsFalse(selectionHistory.IsSelected(0));
			Assert.IsFalse(selectionHistory.IsSelected(1));
			Assert.That(selectionHistory.GetHistoryCount(), Is.EqualTo(2));
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

			selectionHistory.UpdateSelection (selection3);
			Assert.That(selectionHistory.GetSelection(), Is.SameAs(selection3)); 

			selectionHistory.UpdateSelection (selection2);
			Assert.That(selectionHistory.GetSelection(), Is.SameAs(selection2)); 
		}
		
		[Test]
		public void Test_EntriesAreDifferent_WhenNoGameObject()
		{
			var gameObject1 = new GameObject();

			var entry1 = new SelectionHistory.Entry(gameObject1)
			{
				globalObjectId = "TEST_G1",
				reference = null,
				sceneName = "Scene1",
				scenePath = "Assets/Scene1"
			};

			var entry2 = new SelectionHistory.Entry(gameObject1)
			{
				globalObjectId = "TEST_G2",
				reference = null,
				sceneName = "Scene1",
				scenePath = "Assets/Scene1"
			};
			
			Assert.IsFalse(entry1.Equals(entry2));

			// - reference: {fileID: 0}
			// sceneName: GlobalIdTestScene
			// scenePath: Assets/Scenes/GlobalIdTest/GlobalIdTestScene.unity
			// globalObjectId: GlobalObjectId_V1-2-8d557def68244fd46a45dd08bd34aae2-1767799830-0
		}

	}
}