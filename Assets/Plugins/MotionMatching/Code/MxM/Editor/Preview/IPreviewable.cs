// Copyright © 2017-2024 Vault Break Studios Pty Ltd

namespace MxMEditor
{

    public interface IPreviewable
    {
        void BeginPreview();
        void EndPreview();
        void UpdatePreview();
        void EndPreviewLocal();
    }
}