using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class SelectionHistoryTests {

	[Test]
	public void NextAndPreviousShouldntExplodeWithoutItems()
	{
		var navigationWindow = new SelectionHistoryWindow();

		navigationWindow.Previous();
		navigationWindow.Next();

		Assert.Pass();
	}


	[Test]
	public void NavigationWindowTest()
	{
		var navigationWindow = new SelectionHistoryWindow();

		var selection1 = new GameObject();

		navigationWindow.UpdateSelection(selection1);

		Assert.AreSame(navigationWindow.GetSelection(), selection1);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 1);
	}

	[Test]
	public void NavigationWindowTest2()
	{
		var navigationWindow = new SelectionHistoryWindow();

		var selection1 = new GameObject();
		var selection2 = new GameObject();

		navigationWindow.UpdateSelection(selection1);
		navigationWindow.UpdateSelection(selection2);

		Assert.AreSame(navigationWindow.GetSelection(), selection2);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);
	}

	[Test]
	public void UpdateWithSameSelectionShouldntAddTwiceToHistory()
	{
		var navigationWindow = new SelectionHistoryWindow();

		var selection1 = new GameObject();

		navigationWindow.UpdateSelection(selection1);
		navigationWindow.UpdateSelection(selection1);

		Assert.AreSame(navigationWindow.GetSelection(), selection1);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 1);
	}

	[Test]
	public void TestPreviousSelectionShouldntStoreInHistory()
	{
		var navigationWindow = new SelectionHistoryWindow();

		var selection1 = new GameObject();
		var selection2 = new GameObject();

		navigationWindow.UpdateSelection(selection1);
		navigationWindow.UpdateSelection(selection2);

		Assert.AreSame(navigationWindow.GetSelection(), selection2);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);

		navigationWindow.Previous();

		Assert.AreSame(navigationWindow.GetSelection(), selection1);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);

		navigationWindow.Next();

		Assert.AreSame(navigationWindow.GetSelection(), selection2);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);
	}

	[Test]
	public void TestPreviousAndNextShouldntUpdateHistory()
	{
		var navigationWindow = new SelectionHistoryWindow();

		var selection1 = new GameObject();
		var selection2 = new GameObject();

		navigationWindow.UpdateSelection(selection1);
		navigationWindow.UpdateSelection(selection2);

		Assert.AreSame(navigationWindow.GetSelection(), selection2);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);

		navigationWindow.Previous();
		navigationWindow.UpdateSelection(selection1);

		Assert.AreSame(navigationWindow.GetSelection(), selection1);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);

		navigationWindow.Next();
		navigationWindow.UpdateSelection(selection2);

		Assert.AreSame(navigationWindow.GetSelection(), selection2);
		Assert.AreEqual(navigationWindow.GetHistoryCount(), 2);
	}

}
