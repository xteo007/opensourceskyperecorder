using System;
using SKYPE4COMLib;

namespace PublicDomain
{
    public delegate void AttachedCallback(SkypeRecorder skypeRecorder);
    
    public sealed class SkypeAttachment
	{
		private Skype m_skype;
		private SkypeClass m_skypeClass;
		private AttachedCallback m_callBack;
		private SkypeRecorder m_skypeRecorder;

        public SkypeAttachment(AttachedCallback callBack)
		{
			m_callBack = callBack;

			m_skype = new Skype();
			m_skypeRecorder = new SkypeRecorder(m_skype);

			m_skypeClass = new SkypeClass();
			m_skypeClass._ISkypeEvents_Event_AttachmentStatus += new _ISkypeEvents_AttachmentStatusEventHandler(EventAttachmentStatus);
			m_skypeClass.Attach(7, false);

			if (m_skype.Client.IsRunning == false)
			{
				m_skype.Client.Start(false, true);
			}
		}

        public void EventAttachmentStatus(TAttachmentStatus status)
		{
			Console.WriteLine("New Plugin Status: " + m_skypeClass.Convert.AttachmentStatusToText(status));

			switch (status)
			{
				case TAttachmentStatus.apiAttachAvailable:
					m_skypeClass.Attach(7, true);
					break;
				case TAttachmentStatus.apiAttachSuccess:
					m_skype.Attach(7, false);
					m_callBack(m_skypeRecorder);
					break;
			}
		}
	}
}
