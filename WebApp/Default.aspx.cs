using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml.Linq;

using MS.Utility;
using MS.WebUtility;

using Torq.Library;
using Torq.Library.Domain;

using WebApp.master;

namespace WebApp
{
    public partial class Default : UserAuthenticatedPage<AppDC>
    {
        class VideoContent
        {
            public static readonly Product[] All = new Product[] { Product.FullTORQ, Product.cTORQ };
            public static readonly Product[] FullTorqOnly = new Product[] { Product.FullTORQ };
            public static readonly Product[] cTorqOnly = new Product[] { Product.cTORQ };

            public string Title { get; private set; }
            public string Description { get; private set; }
            public string Url { get; private set; }
            public Product[] Products { get; private set; }

            public VideoContent(string title, string url, string description, Product[] products)
            {
                this.Title = title;
                this.Url = url;
                this.Description = description;
                this.Products = products;
            }
        }

        private static VideoContent[] videoContent = {

            new VideoContent("New to cTORQ: Manage your projects more effectively and collaborate with other users in your work group (4:20 min)", "http://static.torqworks.com/tutorials/cTORQ/ProjManPageCC/index.html",
                @"See how long it’s been since your clients have used their personal link and close projects you’re no longer actively managing. Learn how you can view / edit projects created by other users in  your group. This is particularly handy when you need to assist a client of a career counselor who is unavailable.",
                VideoContent.cTorqOnly),
            new VideoContent("Workshops and Multiple Occupations in Work Histories – Two new features for cTORQ (3:40 min)", "http://static.torqworks.com/tutorials/IntroWorkshopsMultiOccsCC/index.html",
                @"cTORQ's new Workshop Mode makes it easy to serve tens of job-seeking clients at once, and the new ability to include multiple occupations greatly improves those clients' efforts to find reemployment.",
                VideoContent.cTorqOnly),
            new VideoContent("TORQ Now Considers Multiple Occupations When Calculating TORQ Scores (3 min)", "http:///static.torqworks.com/tutorials/MultOccsFullTorq/index.html",
                @"TORQ introduces its patented and powerful ability to consider multiple occupations in a job seeker's work history when calculating best reemployment alternatives.  For those with varied work histories, this new feature generates much improved lists of employment options.",
                VideoContent.FullTorqOnly),

            new VideoContent("TORQ 3.1 Released", "http://static.torqworks.com/tutorials/IraPreview/index.html",
                @"See what's recently been added to TORQ.",
                VideoContent.FullTorqOnly),
            new VideoContent("Military Crosswalk", "http://static.torqworks.com/tutorials/MCFinal/MCFinal.html",
                @"See how to translate military occupational codes into O*NET occupations.",
                VideoContent.FullTorqOnly),

            new VideoContent("Home Page", "http://static.torqworks.com/tutorials/Home_Page_with_Caption_Clear/Home_Page_with_Caption_Clear.html",
                "Basic introduction to TORQ and its Home Page.",
                VideoContent.FullTorqOnly),
            new VideoContent("Projects Explained", "http://static.torqworks.com/tutorials/Project_Basics2/Project_Basics2.html",
                @"Projects are a powerful new concept in TORQ 3.0, simplifying workflows and saving your work from one session to another. 
                  This tutorial explains the project concept and provides an overview to the three project types in TORQ today.",
                VideoContent.FullTorqOnly),
            new VideoContent("TORQ Scores", "http://static.torqworks.com/tutorials/TORQ_Scores2/TORQ_Scores2.html",
                @"The relative ease or difficulty of transitioning from one occupation to another is measured by our patent-pending TORQ algorithm,
                  and explained in this tutorial.",
                VideoContent.FullTorqOnly),
            new VideoContent("Create New Project", "http://static.torqworks.com/tutorials/CNP5/CNP5.html",
                @" steps through the process to start a new project from scratch.",
                VideoContent.FullTorqOnly),
            new VideoContent("Project Manager Basics", "http://static.torqworks.com/tutorials/Project_Manager2/Project_Manager2.html",
                @" With the ability to create and save hundreds of projects, you need a simple way to manage your library of projects. 
                   This tutorial describes the Project Manager page and navigation of the site.",
                VideoContent.FullTorqOnly),
            new VideoContent("Single Occupation", "http://static.torqworks.com/tutorials/SO2/SO2.html",
                @"The simplest project type is described here, from creation to the report print-out.",
                VideoContent.FullTorqOnly),
            new VideoContent("Reemployment Analysis", "http://static.torqworks.com/tutorials/RA1/RA1.html",
                @"Job Counseling Sessions and Rapid Response Scenarios are classic reasons to use the Reemployment Analysis project, described here from
                  creation to report generation.",
                VideoContent.FullTorqOnly),
            new VideoContent("Economic Development", "http://static.torqworks.com/tutorials/Ed2-6/Ed2-6.html",
                @"LMI Analysts looking to understand Labor Pools for specific occupations will love the Economic Development project type, 
                  demonstrated here from creation to report generation.",
                VideoContent.FullTorqOnly),
            new VideoContent("Custom Filters", "http://static.torqworks.com/tutorials/CF2-1/CF2-1.html",
                "Harness the power of creating these powerful tools to simply your work process.",
                VideoContent.FullTorqOnly),
            new VideoContent("MyTORQ", "http://static.torqworks.com/tutorials/MyT2/MyT2.html",
                @"MyTORQ allows user to edit information about themselves and to change certain individual preferences. This video explains how to use MyTORQ.",
                VideoContent.FullTorqOnly),
                                                     };

        protected override void OnInit(EventArgs e)
        {
            //!! force over to TORQTouch
            Server.Transfer("/app/default.aspx");
            Debug.Fail("Shouldn't get here");
            base.OnInit(e);
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            Debug.Assert(this.UserIdentity != null);


            // Hide the logo on the side
            TorqMaster tm = this.Master as TorqMaster;
            tm.ShowSideLogo = false;
            tm.ShowTopLogo = true;
            HtmlAnchor helpLink = (HtmlAnchor)tm.FindControl("HelpLink");
            HtmlAnchor subHelpLink = (HtmlAnchor)tm.FindControl("SubHelpLink");
            helpLink.HRef = "/Help.aspx#HomePage";
            subHelpLink.HRef = "/Help.aspx#HomePage";

            string relPath = "/help/TorqInAction.xml";
            string absPath = Server.MapPath(relPath);

            XDocument torqInActionDocument = XDocument.Load(absPath);

            var torqInActionDataSource = torqInActionDocument
                .Descendants("Article")
                .Select(article => new
                {
                    Dateline = article.Element("Dateline").Value,
                    Date = article.Element("Date").Value,
                    Subject = article.Element("Subject").Value,
                });

            this.TorqInActionContents.DataSource = torqInActionDataSource;
            this.TorqInActionContents.DataBind();

            IEnumerable<VideoContent> visibleVideoContent = videoContent
//!! nextgen -                .Where(video => video.Products.Contains(this.UserIdentity.Product));
                .Where(video => video.Products.Contains(Product.cTORQ));

            this.VideoContentList.DataSource = visibleVideoContent;
            this.VideoContentList.DataBind();

            // show workshop button for JCs & cTorq users
            //!! nextgen - if (this.UserIdentity.IsFeatureEnabled(ClientFeatures.Feature.createWorkshop))
            if (true)
                {
                this.ButtonSpacer.Visible = false;
                this.WorkshopButton.Visible = true;
            }
        }
    }
}