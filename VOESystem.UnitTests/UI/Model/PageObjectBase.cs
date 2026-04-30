using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using AutoIt;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Protractor;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.UI.Model
{
    public class PageObjectBase
    {
        //make public for debugging
        protected NgWebDriver _ngWebDriver;
        Dictionary<string, object> _pageElements = new Dictionary<string, object>() { };
        public RedBell RedBell;
        public object AngularController;
        public IWebElement BodyElement;

        public string ExecAngularScript(string scriptContents)
        {
            string retVal = String.Empty;

            object scriptReturn = _ngWebDriver.ExecuteScript(scriptContents, AngularController);
            if (scriptReturn != null)
            {
                retVal = scriptReturn.ToString();
            }

            return retVal;

        }

        public PageObjectBase(NgWebDriver ngWebDriver)
        {
            _ngWebDriver = ngWebDriver;
            RedBell = new RedBell(_ngWebDriver);
            
            AngularController = _ngWebDriver.FindElement(By.Id("container"));
            BodyElement = _ngWebDriver.FindElement(By.TagName("body"));

        }

        public YesNoPopup YesNoPopup
        {
            get
            {
                YesNoPopup popup = new YesNoPopup(this);
                if (popup.IsVisible)
                {
                    return popup;
                }
                else
                {
                    return null;
                }
            }
        }

        public T GetObject<T>(string Id, object[] optionalParams = null, bool persistButton = true)
            where T : ElementObjectBase
        {
            object element;

            if (!_pageElements.TryGetValue(Id, out element) || !persistButton)
            {
                if (optionalParams == null)
                {
                    element = (T)Activator.CreateInstance(typeof(T), _ngWebDriver, _ngWebDriver.FindElement(By.Id(Id)), Id);
                }
                else
                {
                    element = (T)Activator.CreateInstance(typeof(T), _ngWebDriver, _ngWebDriver.FindElement(By.Id(Id)), Id, optionalParams);
                }

                if (persistButton)
                {
                    _pageElements.Add(Id, element);
                }
            }
            
            return (T)element;

        }

        public T GetOption<T>(string Value, string ParentId, string Text)
            where T : ElementOptionBase
        {
            object element;
            object parentElement;

            //need to uniquely identify the option with parentid, value and text jsut in case value is empty then there
            //would be a conflict with the parent object idin the _pageElements list
            if (!_pageElements.TryGetValue(ParentId + Value + Text, out element))
            {
                //find the parent first, assume it exists
                if (!_pageElements.TryGetValue(ParentId, out parentElement)) 
                {
                    element = (T)Activator.CreateInstance(typeof(T), _ngWebDriver,
                        ((NgWebElement)parentElement).FindElement(By.XPath(".//option")));
                    _pageElements.Add(ParentId + Value + Text, element);

                }
            }

            return (T)element;

        }

        public List<T> GetRepeater<T>(string repeaterString, Dictionary<string,string> objectMapping, bool persistRepeater = true)
            where T : new()  //this requires parameterless constructor
        {
            
            object returnList;

            //see if this already defined in the class
            if (!_pageElements.TryGetValue(repeaterString, out returnList) || !persistRepeater)
            {
                
                List<T> newList = new List<T>() { };

                //we know this will be a repeater object but we don't know the structure of the repeat
                IReadOnlyCollection<NgWebElement> repeaterList = _ngWebDriver.FindElements(NgBy.ExactRepeater(repeaterString));

                //get types of all properties
                Dictionary<string, Type> propList = typeof(T).GetProperties().ToDictionary(q => q.Name, q => q.PropertyType);

                foreach (NgWebElement repeatItem in repeaterList)
                {

                    T item = new T();

                    foreach (KeyValuePair<string, string> map in objectMapping)
                    {
                        //get all children of repeater item
                        //key = id
                        //value = PropertyName

                        //find generic web element
                        NgWebElement childWebElement = repeatItem.FindElement(By.Id(map.Key));

                        object childTypedObject = null;
                        string childTypedObjectValue = ifEmpty(childWebElement.Text, childWebElement.GetAttribute("value"));
                        
                        if (propList[map.Value].Namespace.Contains("VOESystem"))
                        {
                            //this is one of our custom types
                            //create the child object of the proper type (//need to check for string, int and the builtin types)
                            childTypedObject = Activator.CreateInstance(
                                propList[map.Value], _ngWebDriver, childWebElement, map.Key);

                        }
                        else if (propList[map.Value] == typeof(string))
                        {
                            //this is a string
                            childTypedObject = childTypedObjectValue;
                        }
                        else if (propList[map.Value] == typeof(Int32))
                        {
                            //this is a string
                            childTypedObject = Int32.Parse(ifEmpty(childTypedObjectValue,"0"));
                        }
                        else
                        {
                            throw new Exception("Unsupported Property type for object " + typeof(T).Name + "." + propList[map.Value].Name);
                        }
                        //set the value of the property to the typed child object
                        typeof(T).GetProperty(map.Value).SetValue(item, childTypedObject);
                    }

                    newList.Add(item);

                }

                if (persistRepeater)
                {
                    _pageElements.Add(repeaterString, newList);
                }
                returnList = newList;
            }


            return (List<T>)returnList;

        }

        public Table GetTable(string Id, string repeaterString, List<string> cellMapping, bool persistTable = true)
        {
            
            //this is getting a table from a repeater
            object returnTable;

            //see if this already defined in the class
            if (!_pageElements.TryGetValue(Id, out returnTable) || !persistTable)
            {

                NgWebElement tableElement = _ngWebDriver.FindElement(By.Id(Id));
                returnTable = (Table)Activator.CreateInstance(typeof(Table), _ngWebDriver, tableElement, Id);

                ((Table)returnTable).TableRows = GetTableRows<Table.TableRow>((Table)returnTable, repeaterString, cellMapping);

                if (persistTable)
                {
                    _pageElements.Add(Id, (Table)returnTable);
                }

            }


            return (Table)returnTable;

        }

        public List<T> GetTableRows<T>(Table tableElement, string repeaterString, List<string> cellMapping, object[] optionalParams = null)
            where T : Table.TableRow
        {
            //we know this will be a repeater object but we don't know the structure of the repeat
            IReadOnlyCollection<NgWebElement> repeaterList = tableElement.FindElements(NgBy.ExactRepeater(repeaterString));

            List<T> returnList = new List<T>() { };

            if (optionalParams == null)
            {
                optionalParams =  new object[] { this };
            }

            int i = 0;
            foreach (NgWebElement repeatItem in repeaterList)  //this corresponds to table rows
            {

                T row = (T)Activator.CreateInstance(
                            typeof(T), _ngWebDriver, repeatItem, i.ToString(), optionalParams);  //id will be just a counter

                foreach (string fieldId in cellMapping)
                {

                    //find web element (td)
                    NgWebElement childWebElement = repeatItem.FindElement(By.Id(fieldId));
                    string cellContents = childWebElement.Text;

                    object childTypedObject = null;

                    //create the child object of the type td
                    childTypedObject = Activator.CreateInstance(
                        typeof(Table.TableRow.TableCell), _ngWebDriver, childWebElement, fieldId);

                    //add cell to row
                    row.TableCells.Add((Table.TableRow.TableCell)childTypedObject);
                }

                returnList.Add(row);
                i++;

            }

            return returnList;
        }

        public List<T> GetObjectGroup<T>(ReadOnlyCollection<NgWebElement> parentElements)
            where T : ElementObjectBase
        {

            object returnList;

            returnList = new List<T>() { };

            //figure out what css selector to use
            ElementCssSelector cssAttr = (ElementCssSelector)Attribute.GetCustomAttribute(typeof(T), typeof(ElementCssSelector));

            if (cssAttr == null)
            {
                throw new ModelExceptions.MissingCSSSelectorAttributeException(typeof(T).Name);
            }

            foreach (NgWebElement parentElement in parentElements)
            {

                //then get child objects and convert to T
                IReadOnlyCollection<NgWebElement> childElements = parentElement.FindElements(By.CssSelector(cssAttr.Name));

                foreach (NgWebElement child in childElements)
                {
                    object element = (T)Activator.CreateInstance(typeof(T), _ngWebDriver, child, null);
                    ((List<T>)returnList).Add((T)element);
                }

            }

            return (List<T>)returnList;

        }

        public List<T> GetObjectGroup<T>(string parentId, bool persistGroup = true)
                    where T : ElementObjectBase
        {
            
            object returnList;

            if (!_pageElements.TryGetValue(parentId, out returnList) || !persistGroup) //this forces refreshing value each time
            {
                //first find parent(s)
                ReadOnlyCollection<NgWebElement> parentElements = _ngWebDriver.FindElements(By.Id(parentId));

                //get group contents
                returnList = GetObjectGroup<T>(parentElements);

                //*****************
                //returnList = new List<T>() { };

                ////then figure out what css selector to use
                //ElementCssSelector cssAttr = (ElementCssSelector)Attribute.GetCustomAttribute(typeof(T), typeof(ElementCssSelector));

                //if (cssAttr == null)
                //{
                //    throw new ModelExceptions.MissingCSSSelectorAttributeException(typeof(T).Name);
                //}

                //foreach (NgWebElement parentElement in parentElements)
                //{

                //    //then get child objects and convert to T
                //    IReadOnlyCollection<NgWebElement> childElements = parentElement.FindElements(By.CssSelector(cssAttr.Name));

                //    foreach (NgWebElement child in childElements)
                //    {
                //        object element = (T)Activator.CreateInstance(typeof(T), _ngWebDriver, child, null);
                //        ((List<T>)returnList).Add((T)element);
                //    }

                //}

                /**********************************/

                if (persistGroup) { 
                    _pageElements.Add(parentId, returnList);
                }

            }

            return (List<T>)returnList;

        }

        public T GetNgGrid<T>(NgWebDriver ngWebDriver, string ngGridAttribute)
            where T : NgGrid
        {
            
            NgWebElement returnGrid = NgGrid.getGridElement<T>(ngWebDriver, ngGridAttribute);

            //get header row and fill
            NgWebElement headerContainer = returnGrid.FindElement(By.ClassName("ngHeaderContainer"));
            IReadOnlyCollection<NgWebElement> headerCols = headerContainer.FindElements(NgBy.ExactRepeater("col in renderedColumns"));

            int i = 0;
            foreach (NgWebElement headerCol in headerCols)
            {
                //sometimes the there will be no text - as in the case where there is only an icon
                //can try to support that later

                NgWebElement header = headerCol.FindElement(By.ClassName("ngHeaderText"));

                string headerLabel = header.Text;
                string colIndex = "ColIndex-" + i.ToString();
                if (headerLabel == "") { headerLabel = colIndex; };

                VOESystem.UnitTests.UI.Tags.NgGrid.NgGridHeaderElement ele = (VOESystem.UnitTests.UI.Tags.NgGrid.NgGridHeaderElement)Activator
                    .CreateInstance(typeof(VOESystem.UnitTests.UI.Tags.NgGrid.NgGridHeaderElement), ngWebDriver, header, colIndex);

                ((T)returnGrid).HeaderRow.Add(headerLabel, ele);

                i++;
            }

            ((T)returnGrid).RowText = NgGrid.getGridRowsText((T)returnGrid);

            return (T)returnGrid;
        }

        public T GetUiGrid<T>(NgWebDriver ngWebDriver, string ngGridAttribute)
            where T : UiGrid
        {

            NgWebElement returnGrid = UiGrid.getGridElement<T>(ngWebDriver, ngGridAttribute);

            //get header row and fill
            NgWebElement headerContainer = returnGrid.FindElement(By.ClassName("ui-grid-header-cell-row"));
            IReadOnlyCollection<NgWebElement> headerCols = headerContainer.FindElements(NgBy.Repeater("col in colContainer.renderedColumns"));

            int i = 0;
            foreach (NgWebElement headerCol in headerCols)
            {
                //sometimes the there will be no text - as in the case where there is only an icon
                //can try to support that later

                //NgWebElement header = headerCol.FindElement(By.ClassName("ui-grid-header-cell"));

                string headerLabel = headerCol.Text.Trim(new char[] { " "[0], "\n"[0], "\r"[0] });
                string colIndex = "ColIndex-" + i.ToString();
                if (headerLabel.Trim() == "") { headerLabel = colIndex; };

                VOESystem.UnitTests.UI.Tags.UiGrid.UiGridHeaderElement ele = (VOESystem.UnitTests.UI.Tags.UiGrid.UiGridHeaderElement)Activator
                    .CreateInstance(typeof(VOESystem.UnitTests.UI.Tags.UiGrid.UiGridHeaderElement), ngWebDriver, headerCol, colIndex);

                ((T)returnGrid).HeaderRow.Add(headerLabel, ele);

                i++;
            }

            ((T)returnGrid).RowText = UiGrid.getGridRowsText((T)returnGrid);

            return (T)returnGrid;
        }

        private string ifEmpty(string value, string replValue)
        {
            if (value == null)
            {
                return replValue;
            }
            else if (value == String.Empty)
            {
                return replValue;
            }
            else
            {
                return value;
            }


        }

        public void WidenElement(IWebElement element, int additionalWidth)
        {
            Actions action = new Actions(_ngWebDriver);
            action.MoveToElement(element, element.Size.Width - 2, 0);
            action.ClickAndHold();
            action.MoveByOffset(additionalWidth, 0);
            action.Release();
            action.Build();
            action.Perform();  
        }

        public bool WaitForAndAcceptAlert(string messageContains = null)
        {
            //this clicks on OK - in the yes/no situation
            return WaitForAndDispatchAlert(messageContains, true);
        }

        public bool WaitForAndDismissAlert(string messageContains = null)
        {
            //this clicks on OK - in the alert-only situation
            return WaitForAndDispatchAlert(messageContains, false);
        }
        
        private bool WaitForAndDispatchAlert(string messageContains, bool isAccept)
        {

            bool retVal = false;

            try
            {
                //sometimes this errors out if it goes too fast
                try { _ngWebDriver.WaitForAngular(); }
                catch { };

                //deal with yesno popup
                if (this.YesNoPopup != null)
                {
                    //click no
                    this.YesNoPopup.NoButton.Click();
                    try { _ngWebDriver.WaitForAngular(); }
                    catch { };
                }

                IAlert alert = _ngWebDriver.SwitchTo().Alert();

                //check the resulting popup does not contain error
                Assert.That(!alert.Text.ToLower().Contains("error"));

                //check that the message does contain messageContains text, if present
                if (messageContains != null)
                {
                    Assert.That(alert.Text.ToLower().Contains(messageContains.ToLower()));
                }

                if (isAccept)
                {
                    alert.Accept();
                }
                else
                {
                    alert.Dismiss();
                }
                
                retVal = true;
            }
            catch (Exception ex)
            {
                //something went wrong
            }

            return retVal;
        }

        public void CloseOpenDialog()
        {
            AutoItX.ControlClick("Open", "Open", "Cancel");
        }

        public void OpenDeveloperTools()
        {
            AutoItX.Sleep(1000);
            AutoItX.Send("{F12}");
            AutoItX.WinSetState("VOE System", "", AutoItX.SW_MAXIMIZE);
        }


    }

    public class ElementObjectBase : NgWebElement 
    {
        public ElementObjectBase()
         : base(null, null) { }

        public ElementObjectBase(NgWebDriver ngWebDriver, IWebElement element, string _Id)
            : base(ngWebDriver, element) 
        { 
            Id = _Id;
            ParentElement = (NgWebElement)element.FindElement(By.XPath(".."));
            _Text = ((NgWebElement)element).Text.Trim();

            if (Id == null || Id == "" )
            {
                //only group items are coming in without ids right now in which case they are wrapped in labels
                Id = element.GetAttribute("Id") ?? ParentElement.Text.Trim();
            }

        }

        public ElementObjectBase(NgWebDriver ngWebDriver, IWebElement element, string _AttributeName, string _AttributeValue)
            : base(ngWebDriver, element)
        {
            //this is for repeater elements
            Id = String.Empty;
            ParentElement = (NgWebElement)element.FindElement(By.XPath(".."));
            _Text = ((NgWebElement)element).Text.Trim();

        }


        public string Id { get; set; }
        public NgWebElement ParentElement { get; set; }
        
        //overriding protractor web element text property
        private string _Text { get; set; }

        public new string Text
        {
            get
            {
                string val = this.GetAttribute("value");

                if (this.WrappedElement.Text == "" && val != null)
                {
                    return val;
                }
                else
                {
                    return _Text;
                }
            }

            set
            {
                _Text = value;
            }
        }

        public bool ReadOnly
        {
            get
            {
                bool isReadOnly = false;
                bool.TryParse(this.GetAttribute("readonly"),out isReadOnly);
                
                return isReadOnly;
            }
        }

        public bool CanEdit
        {
            get
            {
                bool retVal = false;

                if (!this.ReadOnly)
                {
                    if (this.Enabled)
                    {
                        retVal = true;
                    }
                }

                return retVal;
            }
        }

        public void SendKeys(string text, bool clearFirst)
        {

            if (clearFirst)
            {
                this.Clear();
            }

            this.SendKeys(text);

        }

        public void HoverOver(NgWebDriver driver)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(this, this.Size.Width - 2, 0);
            action.Release();
            action.Build();
            action.Perform();
        }


    }

    public class ElementOptionBase : NgWebElement
    {
        public ElementOptionBase()
            : base(null, null) { }

        public ElementOptionBase(NgWebDriver ngWebDriver, IWebElement element, string _value, string _parentId, string _text)
            : base(ngWebDriver, element)
        {
            Value = _value;
            Text = _text;
            ParentId = _parentId;
        }

        public string Value { get; set; }
        public string Text { get; set; }
        public string ParentId { get; set; }

    }

    public class RedBell
    {
        private NgWebElement redBell;

        public RedBell(NgWebDriver ngWebDriver)
        {
            ReadOnlyCollection<NgWebElement> bells = ngWebDriver.FindElements(By.Id("redBell"));

            foreach (NgWebElement bell in bells)
            {
                if (bell.Displayed == true)
                {
                    redBell = bell;
                }

            }

        }

        public void Click()
        {
            redBell.Click();
        }

        public bool IsRed {
            
            get
            {
                bool retVal = false;

                NgWebElement redBellSpan = redBell.FindElement(By.Id("unreadMsgNotification"));

                string redBellClass = redBellSpan.GetAttribute("class");

                if (redBellClass == "on")
                {
                    retVal = true;
                }

                return retVal;

            }
            
        }

        public int UnreadMsgCount {

            get
            {
                int retval = 0;

                NgWebElement redBellToolTip = redBell.FindElement(By.ClassName("inlineToolTip"));

                string redBellText = redBellToolTip.GetAttribute("innerHTML").ToString().Trim();

                if (!redBellText.ToLower().Contains("there are no unread messages"))
                {
                    Regex regex = new Regex(@"(\d+)");
                    retval = Int32.Parse(regex.Match(redBellText).Value);
                }

                return retval;
            }
        }
    }

    public class YesNoPopup
    {

        public Button YesButton = null;
        public Button NoButton = null;
        public string PopupText = null;

        public YesNoPopup(PageObjectBase page) 
        {
            try
            {
                YesButton = page.GetObject<Button>("yesButton", null, false);
                NoButton = page.GetObject<Button>("noButton", null, false);
                PopupText = page.GetObject<Div>("yesNoText", null, false).Text;
            }
            catch { }
        }

        public bool IsVisible
        {
            get
            {
                return YesButton != null;
            }
        }

    }
    
    public class ElementCssSelector : Attribute
    {
        private string _Name;

        public ElementCssSelector(string name)
        {
            this._Name = name;
        }

        public virtual string Name
        {
            get { return _Name; }
        }

    }

    public class ModelException : VOESystem.UnitTests.Business.BusinessBase.CustomException
        {

            public ModelException() { }

            public ModelException(string message)
                : base(message) { }
        }

    public static partial class ModelExceptions
    {
        public class MissingCSSSelectorAttributeException : ModelException
        {
            public string TypeName;

            public override string Message
            {
                get
                {
                    return "Object Type " + this.TypeName + " missing CSSSelector Attribute";
                }
            }

            public MissingCSSSelectorAttributeException(string message)
                : base(message) 
            {
                TypeName = message;
            }

        }


    }
}
