/*
	Copyright © Bryan Apellanes 2015  
*/
using System;
using Bam.Net.Presentation;
using System.Collections.Generic;
using Bam.Net.ServiceProxy;

namespace Bam.Net.Presentation
{
    public interface ITemplateManager: ITemplateRenderer
	{
		void SetContentType(IResponse response);

        
        string ContentRoot { get; }
		void RenderLayout(LayoutModel toRender, System.IO.Stream output);
        void EnsureDefaultTemplate(Type type);
	}
}
