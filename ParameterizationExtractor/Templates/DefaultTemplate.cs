﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 14.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Quipu.ParameterizationExtractor.Templates
{
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Quipu.ParameterizationExtractor.Model;
    using Quipu.ParameterizationExtractor.Common;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "14.0.0.0")]
    public partial class DefaultTemplate : DefaultTemplateBase
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("  \r\n");
            this.Write("\r\n/*\r\nAutogenerated by excavator. \r\n*/\r\n\r\nGO\r\n\r\nbegin try\r\nbegin tran\r\n\r\n\r\n");
            
            #line 21 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 var items = this.Session["source"] as IEnumerable<PTable>; 
            
            #line default
            #line hidden
            
            #line 22 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 foreach(var item in items) 
{	
	ProcessRelation(item, item.Childern);
}

            
            #line default
            #line hidden
            this.Write(@"

commit

END TRY
BEGIN CATCH
	 DECLARE @szErrorMessage NVARCHAR(4000)
	 SET @szErrorMessage = 'LN: ' + CAST(ERROR_LINE() AS NVARCHAR(20)) + N', ' + CHAR(13) + ERROR_MESSAGE()
	 WHILE @@trancount > 0
		  ROLLBACK TRAN
	 RAISERROR(@szErrorMessage, 16, 1)
END CATCH


");
            return this.GenerationEnvironment.ToString();
        }
        
        #line 41 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
  	
	void InsertValues(IEnumerable<PField> fields, string tableName)
	{

	
        
        #line default
        #line hidden
        
        #line 45 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\tinsert into ");

        
        #line default
        #line hidden
        
        #line 46 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(tableName);
        
        #line default
        #line hidden
        
        #line 46 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(" (");

        
        #line default
        #line hidden
        
        #line 46 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(string.Join(",",fields.Select(_=>string.Format("[{0}]",_.FieldName))));
        
        #line default
        #line hidden
        
        #line 46 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(")\r\n\tvalues (");

        
        #line default
        #line hidden
        
        #line 47 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(string.Join(",",fields.Select(_=>_.ValueToSqlString())));
        
        #line default
        #line hidden
        
        #line 47 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(")\r\n\t");

        
        #line default
        #line hidden
        
        #line 48 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"

	}

	void IfNotExists(PTable table)
	{
	
        
        #line default
        #line hidden
        
        #line 53 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\tif not exists(select top 1 1 from ");

        
        #line default
        #line hidden
        
        #line 54 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(string.Format("{0} {1}",table.TableName,table.GetUniqueSqlWhere()));
        
        #line default
        #line hidden
        
        #line 54 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(" )\t\t\t\t\r\n\t");

        
        #line default
        #line hidden
        
        #line 55 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"

	}

        
        #line default
        #line hidden
        
        #line 58 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
  	
	void ProcessRelation(PTable parent, IEnumerable<PTableDependency> children)  
    {  
		//this.Write(SqlHelper.IfExistsSql(parent));
IfNotExists(parent);
		var fields = SqlHelper.NotIdentityFields(parent);
		
        
        #line default
        #line hidden
        
        #line 64 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\tBegin\r\n\t\tdeclare ");

        
        #line default
        #line hidden
        
        #line 66 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 Write(string.Format("{0} {1}",parent.GetPKVarName(), parent.PkField.MetaData.SqlType));
        
        #line default
        #line hidden
        
        #line 66 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t");

        
        #line default
        #line hidden
        
        #line 67 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
InsertValues(fields,parent.TableName); 
        
        #line default
        #line hidden
        
        #line 67 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t\t");

        
        #line default
        #line hidden
        
        #line 68 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 if (parent.PkField.MetaData.IsIdentity) 
				{
        
        #line default
        #line hidden
        
        #line 69 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t\t\t\t \r\n\t\tset ");

        
        #line default
        #line hidden
        
        #line 70 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 Write(parent.GetPKVarName());
        
        #line default
        #line hidden
        
        #line 70 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(" = SCOPE_IDENTITY()  \r\n\t\t\t");

        
        #line default
        #line hidden
        
        #line 71 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 } 
				else if (parent.PkField.MetaData.FieldType.IsNumericType() )
				{ 
        
        #line default
        #line hidden
        
        #line 73 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t\t\t\tselect ");

        
        #line default
        #line hidden
        
        #line 74 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(parent.GetPKVarName());
        
        #line default
        #line hidden
        
        #line 74 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(" = max(");

        
        #line default
        #line hidden
        
        #line 74 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(parent.PkField.FieldName);
        
        #line default
        #line hidden
        
        #line 74 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write(")+1 \r\n\t\t\t\t\tfrom ");

        
        #line default
        #line hidden
        
        #line 75 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
Write(parent.TableName);
        
        #line default
        #line hidden
        
        #line 75 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t\t\t");

        
        #line default
        #line hidden
        
        #line 76 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
}
			
        
        #line default
        #line hidden
        
        #line 77 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\t\t\t\t\r\n\t\t\t");

        
        #line default
        #line hidden
        
        #line 78 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
 
				foreach(var child in children)
				{
					var childFields = SqlHelper.PrepareFieldsForChild(child.PTable, parent.GetPKVarName() , child.FK);
					//IfNotExists(child);
					InsertValues(childFields,child.PTable.TableName);
				}
			
        
        #line default
        #line hidden
        
        #line 85 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"
this.Write("\tEnd\r\n\t\t");

        
        #line default
        #line hidden
        
        #line 87 "C:\MyProjects\ParameterizationExtractor\ParameterizationExtractor\ParameterizationExtractor\Templates\DefaultTemplate.tt"

    }  

        
        #line default
        #line hidden
    }
    
    #line default
    #line hidden
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "14.0.0.0")]
    public class DefaultTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
