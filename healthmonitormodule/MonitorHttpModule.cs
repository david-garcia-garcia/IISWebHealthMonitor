using System;
using System.Web;
using System.IO;
using System.Net;
using healthmonitormodule;

/// <summary>
/// This was an attempt to use an HTTP module to implement site monitoring in a more robust way
/// </summary>
public class MonitorHttpModule : IHttpModule
{
    public MonitorHttpModule()
    {
    }

    public String ModuleName
    {
        get { return "MonitorHttpModule"; }
    }

    StreamSizeWatcher _watcher = null;

    // In the Init function, register for HttpApplication 
    // events by adding your handlers.
    public void Init(HttpApplication application)
    {
        application.BeginRequest +=
            (new EventHandler(this.Application_BeginRequest));

        application.EndRequest +=
            (new EventHandler(this.Application_EndRequest));

        application.PreRequestHandlerExecute += Application_PreRequestHandlerExecute;
    }

    private void Application_PreRequestHandlerExecute(object sender, EventArgs e)
    {
        try
        {

            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            _watcher = new StreamSizeWatcher(context.Response.Filter);
            context.Response.Filter = _watcher;

        }
        catch (Exception ex)
        {
            try
            {
                var logger = new healthmonitorlogger.Logger();
                logger.LogError(String.Format("Exception: {0}", ex.Message));
            }
            catch { }
        }
    }

    private void Application_BeginRequest(Object source,
         EventArgs e)
    {
        try
        {

            // Create HttpApplication and HttpContext objects to access
            // request and response properties.
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            _watcher = new StreamSizeWatcher(context.Response.Filter);
            context.Response.Filter = _watcher;

        }
        catch (Exception ex)
        {
            try
            {
                var logger = new healthmonitorlogger.Logger();
                logger.LogError(String.Format("Exception: {0}", ex.Message));
            }
            catch { }
        }
    }

    private void Application_EndRequest(Object source, EventArgs e)
    {
        try
        {

            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;

            context.Response.Write("<br/><H1>GOTCHA: " + _watcher.getSize() + "</H1>");

            if (_watcher != null)
            {
                var size = _watcher.getSize();

                // Do not log response larger than 1Kb.
                if (size > 1024)
                {
                    return;
                }



                var logger = new healthmonitorlogger.Logger();
                logger.LogError(String.Format("Bad size response detected. \r\n Size: {0} \r\n Uri: {1}", size, context.Request.Path));

            }

        }
        catch (Exception ex)
        {
            try
            {
                var logger = new healthmonitorlogger.Logger();
                logger.LogError(String.Format("Exception: {0}", ex.Message));
            }
            catch { }
        }

    }

    public void Dispose() { }
}