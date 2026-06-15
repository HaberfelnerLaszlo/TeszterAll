window.rteInterop = {
  registerChangeHandler: function (wrapperId, dotNetRef) {
    try {
      const wrapper = document.getElementById(wrapperId);
      if (!wrapper) return;
      // Quill editor area általában .ql-editor osztályú elem
      const editor = wrapper.querySelector('.ql-editor');
      if (!editor) return;

      // Debounce segéd
      let timeout;
      const handler = () => {
        clearTimeout(timeout);
        timeout = setTimeout(() => {
          dotNetRef.invokeMethodAsync('NotifyEditorChanged');
        }, 400); // pl. 400ms debounce
      };

      editor.addEventListener('input', handler);
      editor.addEventListener('keyup', handler);

      // tároljuk a handler referenciát, hogy késõbb eltávolítható legyen
      wrapper.__rteHandler = handler;
    } catch (e) {
      console.error(e);
    }
  },

  unregisterChangeHandler: function (wrapperId) {
    try {
      const wrapper = document.getElementById(wrapperId);
      if (!wrapper) return;
      const editor = wrapper.querySelector('.ql-editor');
      const handler = wrapper.__rteHandler;
      if (editor && handler) {
        editor.removeEventListener('input', handler);
        editor.removeEventListener('keyup', handler);
      }
      wrapper.__rteHandler = null;
    } catch (e) {
      console.error(e);
    }
    },
    setBeforeUnload: function (enable) {
        if (enable) {
            window.onbeforeunload = function (e) {
                e = e || window.event;
                // Chrome requires returnValue to be set
                if (e) e.returnValue = 'You have unsaved changes. Are you sure you want to leave?';
                return 'You have unsaved changes. Are you sure you want to leave?';
            };
        } else { window.onbeforeunload = null; }
    }
};