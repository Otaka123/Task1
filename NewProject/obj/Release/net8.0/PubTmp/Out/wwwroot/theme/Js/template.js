(function($) {
  'use strict';
  $(function() {
    var body = $('body');
    var contentWrapper = $('.content-wrapper');
    var scroller = $('.container-scroller');
    var footer = $('.footer');
    var sidebar = $('.sidebar');

    //Add active class to nav-link based on url dynamically
    //Active class can be hard coded directly in html file also as required

      function addActiveClass(element) {
          // خُذ آخر جزء من الـ URL الحالي
          var pathParts = location.pathname.split("/").filter(Boolean);
          var current = pathParts.slice(-1)[0];

          // ✅ تحقق إذا كان current undefined أو فارغ
          if (!current || current === "" || current === "undefined") {
              return;
          }

          element.each(function () {
              var $this = $(this);
              var href = $this.attr("href");

              // ✅ تجاهل الروابط الفارغة أو اللي فيها "#"
              if (!href || href === "#" || href.trim() === "") {
                  return;
              }

              // ✅ استخرج آخر جزء من الرابط
              var hrefParts = href.split("/").filter(Boolean);
              var lastHrefPart = hrefParts.slice(-1)[0];

              // ✅ تحقق إذا كان lastHrefPart غير undefined
              if (!lastHrefPart || lastHrefPart === "" || lastHrefPart === "undefined") {
                  return;
              }

              // ✅ استخدم indexOf بأمان مع تحقق إضافي
              if (current && lastHrefPart && current.indexOf(lastHrefPart) !== -1) {
                  $this.parents(".nav-item").last().addClass("active");

                  if ($this.parents(".sub-menu").length) {
                      $this.closest(".collapse").addClass("show");
                      $this.addClass("active");
                  }
              }
          });
      }





    var current = location.pathname.split("/").slice(-1)[0].replace(/^\/|\/$/g, '');
    $('.nav li a', sidebar).each(function() {
      var $this = $(this);
      addActiveClass($this);
    })

    $('.horizontal-menu .nav li a').each(function() {
      var $this = $(this);
      addActiveClass($this);
    })

    //Close other submenu in sidebar on opening any

    sidebar.on('show.bs.collapse', '.collapse', function() {
      sidebar.find('.collapse.show').collapse('hide');
    });


    //Change sidebar and content-wrapper height
    applyStyles();

    function applyStyles() {
      //Applying perfect scrollbar
      if (!body.hasClass("rtl")) {
        if ($('.settings-panel .tab-content .tab-pane.scroll-wrapper').length) {
          const settingsPanelScroll = new PerfectScrollbar('.settings-panel .tab-content .tab-pane.scroll-wrapper');
        }
        if ($('.chats').length) {
          const chatsScroll = new PerfectScrollbar('.chats');
        }
        if (body.hasClass("sidebar-fixed")) {
          if($('#sidebar').length) {
            var fixedSidebarScroll = new PerfectScrollbar('#sidebar .nav');
          }
        }
      }
    }

    $('[data-toggle="minimize"]').on("click", function() {
      if ((body.hasClass('sidebar-toggle-display')) || (body.hasClass('sidebar-absolute'))) {
        body.toggleClass('sidebar-hidden');
      } else {
        body.toggleClass('sidebar-icon-only');
      }
    });

    //checkbox and radios
    $(".form-check label,.form-radio label").append('<i class="input-helper"></i>');

    //Horizontal menu in mobile
    $('[data-toggle="horizontal-menu-toggle"]').on("click", function() {
      $(".horizontal-menu .bottom-navbar").toggleClass("header-toggled");
    });
    // Horizontal menu navigation in mobile menu on click
    var navItemClicked = $('.horizontal-menu .page-navigation >.nav-item');
    navItemClicked.on("click", function(event) {
      if(window.matchMedia('(max-width: 991px)').matches) {
        if(!($(this).hasClass('show-submenu'))) {
          navItemClicked.removeClass('show-submenu');
        }
        $(this).toggleClass('show-submenu');
      }        
    })

    $(window).scroll(function() {
      if(window.matchMedia('(min-width: 992px)').matches) {
        var header = $('.horizontal-menu');
        if ($(window).scrollTop() >= 70) {
          $(header).addClass('fixed-on-scroll');
        } else {
          $(header).removeClass('fixed-on-scroll');
        }
      }
    });
    
  });
})(jQuery);