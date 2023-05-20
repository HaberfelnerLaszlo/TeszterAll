// This is a JavaScript module that is loaded on demand. It can export any number of
// functions, and may import other JavaScript modules if required.

export function showPrompt(message) {
  return prompt(message, 'Type anything here');
}
if (!window.TinyMce) {
    window.TinyMce = {};
}

window.TinyMce = {
    params: {
        standard: {
            plugins: 'code codesample link image autolink lists media paste table table spellchecker',
            toolbar1: 'undo redo | paste | removeformat styleselect | bold italic | alignleft aligncenter alignright alignjustify | outdent indent | link image media codesample | table | code | spellchecker',
            menubar: false,
            branding: false,
            toolbar_mode: 'floating'
        },
        minimal: {
            toolbar1: 'bold italic underline',
            menubar: false,
            branding: false,
            toolbar_mode: 'floating'
        },
        grouped: {
            plugins: "emoticons hr image link lists charmap table",
            toolbar: "formatgroup paragraphgroup insertgroup",
            toolbar_groups: {
                formatgroup: {
                    icon: 'format',
                    tooltip: 'Formatting',
                    items: 'bold italic underline strikethrough | forecolor backcolor | superscript subscript | removeformat'
                },
                paragraphgroup: {
                    icon: 'paragraph',
                    tooltip: 'Paragraph format',
                    items: 'h1 h2 h3 | bullist numlist | alignleft aligncenter alignright alignjustify | indent outdent'
                },
                insertgroup: {
                    icon: 'plus',
                    tooltip: 'Insert',
                    items: 'link image emoticons charmap hr'
                }
            },
            menubar: false,
            branding: false
        },
        bloated: {
            plugins: 'code codesample link image autolink lists media paste table table spellchecker',
            toolbar1: 'undo redo | styleselect | forecolor | bold italic underline strikethrough | link image media codesample | table | code | spellchecker',
            toolbar2: 'h1 h2 h3 | bullist numlist | alignleft aligncenter alignright alignjustify | outdent indent | emoticons charmap hr',
            menubar: false,
            branding: false,
            toolbar_mode: 'floating'
        }
    },
    init: function (id, mode, dotnetHelper) {
        var params = window.TinyMce.params[mode];
        params.selector = '#' + id;

        params.setup = function (editor) {
            editor.on('change', function (e) {
                console.log($('#' + id).val());
                $('#' + id).val(editor.getContent());
                $('#' + id).change();
                console.log($('#' + id).val());
                dotnetHelper.invokeMethodAsync("textArea_OnChange", $('#' + id).val());
            });
        }

        tinymce.init(params);
    },
    dispose: function (id, dotnetHelper) {
        tinymce.remove('#' + id);
    }
};
window.editor = {
    height: 300,
    language: 'hu_HU',
    //plugins: 'image code',
    //toolbar: 'undo redo | link image | code',
    /* enable title field in the Image dialog*/
    image_title: true,
    /* enable automatic uploads of images represented by blob or data URIs*/
    automatic_uploads: true,
    images_upload_url: '/TinyMCE/Upload',
    file_picker_types: 'image',
    /* and here's our custom image picker*/
    file_picker_callback: function (cb, value, meta) {
        var input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');
        input.onchange = function () {
            var file = this.files[0];

            var reader = new FileReader();
            reader.onload = function () {
                var id = 'blobid' + (new Date()).getTime();
                var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                var base64 = reader.result.split(',')[1];
                var blobInfo = blobCache.create(id, file, base64);
                blobCache.add(blobInfo);

                /* call the callback and populate the Title field with the file name */
                cb(blobInfo.blobUri(), { title: file.name });
            };
            reader.readAsDataURL(file);
        };

        input.click();
    },
    content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }'
};
window.getDimensions = {
    win: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },
    comp: function (element) {
        var el = document.getElementById(element)
        if (el == null) {
            return {
                width: -1,
                height: -1
            }
        }
        return {
            width: el.clientWidth,
            height: el.clientHeight
        }
    }
};