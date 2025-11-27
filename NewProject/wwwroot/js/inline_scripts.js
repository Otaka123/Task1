
    tinymce.init({
      selector: '#details',
      plugins: [
      'advlist', 'autolink', 'link', 'image', 'lists', 'charmap', 'preview', 'anchor', 'pagebreak',
      'searchreplace', 'wordcount', 'visualblocks', 'code', 'fullscreen', 'insertdatetime', 'media',
      'table', 'emoticons', 'template', 'help'
    ],
    toolbar: 'undo redo | styles | bold italic | alignleft aligncenter alignright alignjustify | ' +
      'bullist numlist outdent indent | link image | print preview media fullscreen | ' +
      'forecolor backcolor emoticons | help',
    menu: {
      favs: { title: 'My Favorites', items: 'code visualaid | searchreplace | emoticons' }
    },
    menubar: 'favs file edit view insert format tools table help',

      image_title: true,
      /* enable automatic uploads of images represented by blob or data URIs*/
      automatic_uploads: true,
      file_picker_types: 'image',
      /* and here's our custom image picker*/
      file_picker_callback: function(cb, value, meta) {
        var input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');

        /*
          Note: In modern browsers input[type="file"] is functional without
          even adding it to the DOM, but that might not be the case in some older
          or quirky browsers like IE, so you might want to add it to the DOM
          just in case, and visually hide it. And do not forget do remove it
          once you do not need it anymore.
        */

        input.onchange = function() {
          var file = this.files[0];

          var reader = new FileReader();
          reader.onload = function() {
            /*
              Note: Now we need to register the blob in TinyMCEs image blob
              registry. In the next release this part hopefully won't be
              necessary, as we are looking to handle it internally.
            */
            var id = 'blobid' + (new Date()).getTime();
            var blobCache = tinymce.activeEditor.editorUpload.blobCache;
            var base64 = reader.result.split(',')[1];
            var blobInfo = blobCache.create(id, file, base64);
            blobCache.add(blobInfo);

            /* call the callback and populate the Title field with the file name */
            cb(blobInfo.blobUri(), {
              title: file.name
            });
          };
          reader.readAsDataURL(file);
        };

        input.click();
      },
    });




      tinymce.init({
      selector: '#details_ar',
        plugins: [
      'advlist', 'autolink', 'link', 'image', 'lists', 'charmap', 'preview', 'anchor', 'pagebreak',
      'searchreplace', 'wordcount', 'visualblocks', 'code', 'fullscreen', 'insertdatetime', 'media',
      'table', 'emoticons', 'template', 'help'
    ],
    toolbar: 'undo redo | styles | bold italic | alignleft aligncenter alignright alignjustify | ' +
      'bullist numlist outdent indent | link image | print preview media fullscreen | ' +
      'forecolor backcolor emoticons | help',
    menu: {
      favs: { title: 'My Favorites', items: 'code visualaid | searchreplace | emoticons' }
    },
    menubar: 'favs file edit view insert format tools table help',



      image_title: true,
          directionality : 'rtl',

      /* enable automatic uploads of images represented by blob or data URIs*/
      automatic_uploads: true,
      file_picker_types: 'image',
      /* and here's our custom image picker*/
      file_picker_callback: function(cb, value, meta) {
        var input = document.createElement('input');
        input.setAttribute('type', 'file');
        input.setAttribute('accept', 'image/*');

        /*
          Note: In modern browsers input[type="file"] is functional without
          even adding it to the DOM, but that might not be the case in some older
          or quirky browsers like IE, so you might want to add it to the DOM
          just in case, and visually hide it. And do not forget do remove it
          once you do not need it anymore.
        */

        input.onchange = function() {
          var file = this.files[0];

          var reader = new FileReader();
          reader.onload = function() {
            /*
              Note: Now we need to register the blob in TinyMCEs image blob
              registry. In the next release this part hopefully won't be
              necessary, as we are looking to handle it internally.
            */
            var id = 'blobid' + (new Date()).getTime();
            var blobCache = tinymce.activeEditor.editorUpload.blobCache;
            var base64 = reader.result.split(',')[1];
            var blobInfo = blobCache.create(id, file, base64);
            blobCache.add(blobInfo);

            /* call the callback and populate the Title field with the file name */
            cb(blobInfo.blobUri(), {
              title: file.name
            });
          };
          reader.readAsDataURL(file);
        };

        input.click();
      },
    });
  


        window.setTimeout(function() {
            $(".alert").fadeTo(500, 0).slideUp(500, function() {
                $(this).remove();
            });
        }, 4000);
    


        function printContent() {
            var printContents = document.getElementById("print-content").outerHTML;
            var originalContents = document.body.innerHTML;

            // Replace the page content with the print content
            document.body.innerHTML = printContents;

            // Print the page
            window.print();

            // Restore the original page content
            document.body.innerHTML = originalContents;
        }
    


        $('input[name="selectedIds[]"]').change(function() {
            var submitBtn = $('#submit_prog');
            if ($('input[name="selectedIds[]"]:checked').length > 0) {
                submitBtn.show();
            } else {
                submitBtn.hide();
            }
        });
        $('input[name="all_check"]').change(function() {
            var submitBtn = $('#submit_prog');
            if ($('input[name="all_check"]:checked').length > 0) {
                submitBtn.show();
            } else {
                submitBtn.hide();
            }
        });
    


    setSelectHover();

    function setSelectHover(selector = "select") {
        let selects = document.querySelectorAll(selector);
        selects.forEach((select) => {
            let selectWrap = select.parentNode.closest(".select-wrap");
            // wrap select element if not previously wrapped
            if (!selectWrap) {
                selectWrap = document.createElement("div");
                selectWrap.classList.add("select-wrap");
                select.parentNode.insertBefore(selectWrap, select);
                selectWrap.appendChild(select);
            }
            // set expanded height according to options
            let size = select.querySelectorAll("option").length;

            // adjust height on resize
            const getSelectHeight = () => {
                selectWrap.style.height = "auto";
                let selectHeight = select.getBoundingClientRect();
                selectWrap.style.height = selectHeight.height + "px";
            };
            getSelectHeight(select);
            window.addEventListener("resize", (e) => {
                getSelectHeight(select);
            });

            /**
             * focus and click events will coincide
             * adding a delay via setTimeout() enables the handling of
             * clicks events after the select is focused
             */
            let hasFocus = false;
            select.addEventListener("focus", (e) => {
                select.setAttribute("size", size);
                setTimeout(() => {
                    hasFocus = true;
                }, 150);
            });

            // close select if already expanded via focus event
            select.addEventListener("click", (e) => {
                if (hasFocus) {
                    select.blur();
                    hasFocus = false;
                }
            });

            // close select if selection was set via keyboard controls
            select.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    select.removeAttribute("size");
                    select.blur();
                }
            });

            // collapse select
            select.addEventListener("blur", (e) => {
                select.removeAttribute("size");
                hasFocus = false;
            });
        });
    }
