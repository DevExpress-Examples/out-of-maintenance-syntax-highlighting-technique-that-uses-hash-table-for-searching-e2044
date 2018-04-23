# Syntax highlighting technique that uses hash table for searching


<p>Please review the  <a href="https://www.devexpress.com/Support/Center/p/E2993">E2993: Syntax highlighting for C# and VB code using DevExpress CodeParser and Syntax Highlight tokens</a> example instead, if you use <strong>v2010 vol.2.6</strong><strong> and later</strong>.</p><p>This example illustrates the use of the service based on the ISyntaxHighlightService interface. This service is called every time the document content is changed. Then you can analyze the content and highlight required words in a document.<br />
The <strong>Execute</strong> method of the service uses hash table  instead of the straightforward search.<br />
</p>

<br/>


