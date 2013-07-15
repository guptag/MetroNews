window.$Config = {
    DefaultCtg: "Headlines",
    NewsCanvasMinW: 320,
    NewsItemRect: { w: 305, h: 155 },
    ArticleCount: { min: 15, max: 30 }
};

window._ge = function (p_el) { return document.getElementById(p_el); };

(function () {
    var w = window;

    w.$PageController = function (p_data) {
        var _this = this;
        var _data;
        var _categoiesModel, _newsDataModel;
        var _topNavView, _newsContainerView;

        constr();

        function constr() {
            _data = p_data;

            // initialize the models
            _categoiesModel = new $CategoriesModel(_data.Categories);
            _newsDataModel = new $NewsDataModel(_data);

            // initialize the views
            _topNavView = new $TopNavView(_categoiesModel, "nav#topmenu > ul", { "onchange": onNavChanged });
            _newsContainerView = new $NewsContainerView(_newsDataModel, "#topcontainer");

            var _window = $(window);
            _window.bind("resize.app", function () {
                _newsContainerView.arrange();
            });

            _window.bind("unload.app", function () {
                _topNavView.dispose();
                _newsContainerView.dispose();
                _window.unbind(".app");
            });


        }

        _this.bootstrap = function () {
            _topNavView.render();
            _newsContainerView.render();
        }

        function onNavChanged(p_category) {
            _newsContainerView.render(p_category);
        }
    }
})();


(function () {
    var w = window;
    w.$NewsContainerView = function () {
        var _this = this;
        var _model = arguments[0];
        var _selector = arguments[1];
        var _activeCategory;

        _this.render = function (p_category) {
            if (!p_category) {
                p_category = $Config.DefaultCtg;
            }

            // inital render after pageload
            if (!_activeCategory) {
                _activeCategory = $Config.DefaultCtg;
                generateLayout();
            }
            else {
                //switching to a different category view
                //hide the current category (via shrinkandslide animation)
                //and then render the new selected category view
                $AnimHelper.shrinkAndSlide("#" + _activeCategory)
                           .then(function () {
                               $("#" + _activeCategory)
                                        .css({ "display": "none", "opacity": 0 })
                                        .removeClass("newsreveal");
                               _activeCategory = p_category;
                               generateLayout();
                           });

            }
        }

        _this.arrange = function (p_initialRendering, p_layoutData) {
            if (!_ge(_activeCategory)) return;

            if (!p_layoutData) {
                p_layoutData = getLayoutDimensions();
            }

            var _newsItemsPerRow = p_layoutData.itemsPerRow;
            var _containerPadding = p_layoutData.containerPadding;

            if (!p_initialRendering) {
                //mark all the container layouts as dirty (due to window resize) except the activecontainer  
                //arrange method needs to be executed when ever the specific containers are shown
                $("div.sectioncontainer", "#topcontainer").each(function (p_i) {
                    var $el = $(this);
                    $el.attr("data-arrange", $el.attr("id") === _activeCategory ? "1" : "-1");
                });
            }

            var _$newsItems = $("#" + _activeCategory + " div.news");
            _$newsItems.each(function (p_i) {
                $(this).css(
                            { "left": (p_i % _newsItemsPerRow) * $Config.NewsItemRect.w,
                                "top": Math.floor(p_i / _newsItemsPerRow) * $Config.NewsItemRect.h
                            })
                       .addClass(!p_initialRendering ? "newsAnim" : "");
                //animation is needed only during browser resize (not during initial rendering)
                //having "newsAdmin" class kicks off transitions when position is changed
            });


            //add height/margin based on the total news tiles            
            $("#" + _activeCategory).css({ "margin-left": _containerPadding,
                "height": Math.ceil(_$newsItems.length / _newsItemsPerRow) * $Config.NewsItemRect.h,
                "width": _newsItemsPerRow * $Config.NewsItemRect.w
            });
        }

        _this.dispose = function () {

        }

        function generateLayout() {
            if (!_ge(_activeCategory)) {
                var _layoutData = getLayoutDimensions();
                var _newsItems = _model.yieldArticles(_activeCategory, _layoutData.totalItemCount);

                // ensure container exists for the current category
                $("<div></div>").attr("id", _activeCategory)
                                .attr("data-arrange", "-1") //indicates that layout needs to rearranged
                                .css({ "opacity": 0 })
                                .addClass("sectioncontainer newsreveal")
                                .appendTo($("#topcontainer"));


                $.each(_newsItems, function (p_i, p_news) {
                    renderNewsItem(p_news,
                                   {
                                       left: 0,
                                       top: 0
                                   },
                                   ["#", _activeCategory].join(""));
                });

                _this.arrange(true, _layoutData);
                $("#" + _activeCategory).css({ "opacity": "1" });
            }
            else {
                var _$container = $("#" + _activeCategory);

                _$container.addClass("newsreveal")
                           .css({ "display": "block" });

                //window was resized after the last arrange
                if (_$container.attr("data-arrange") == "-1") {
                    _this.arrange(false);
                }

                setTimeout(function () {
                    // opacity animation will kick in (defined in 'newsreveal')
                    _$container.css({ "opacity": 1 });
                }, 10);

            }
        }

        function getLayoutDimensions() {
            var _window = $(window);
            var _windowW = _window.width();
            var _windowH = _window.height();
            var _containerH = (_windowH - $("#topcontainer").offset().top) << 0;
            var _containerW = (_windowW < $Config.NewsCanvasMinW) ? $Config.NewsCanvasMinW : _windowW;
            var _newsItemsPerRow = (_containerW / $Config.NewsItemRect.w) << 0; //converts float to int
            var _containerPadding = (_containerW % ($Config.NewsItemRect.w * _newsItemsPerRow)) / 2;
            var _totalItemCount = ((_containerH / $Config.NewsItemRect.h) << 0) * _newsItemsPerRow;
            var _minArticles = $Config.ArticleCount.min;
            var _maxArticles = $Config.ArticleCount.max;


            return {
                containerW: _containerW,
                containerPadding: _containerPadding,
                itemsPerRow: _newsItemsPerRow,
                totalItemCount: (_totalItemCount < _minArticles) ? _minArticles :
                                                          (_totalItemCount > _maxArticles ? _maxArticles : _totalItemCount)
            }

        }

        function renderNewsItem(p_newsItem, p_pos, p_containerSelector) {
            var _newsTemplate = $("#newsTemplate").html();
            $(_newsTemplate)
                .find("a.titleclick:first")
                .attr("href", p_newsItem.Link)
                .text(p_newsItem.Title)
                .end().find("a.source:first")  //back to root and find a.source
                .text("Source: " + p_newsItem.Source.Title)
                .attr("href", p_newsItem.Source.Link || "#")
                .end().find("a.coverage:first")	 //back to root and find a.coverage								
                .attr("href", "http://www.bing.com/news/search?q=" + encodeURIComponent(p_newsItem.Title))
                .end().find("a.newsimgclick:first") //back to root and find a.newsimgclick
                .attr("href", p_newsItem.Link)
                .find("img.newsimg:first") //find img.newsimg within a.newsimgclick
                .load(function () {
                    var _$img = $(this);
                    adjustAspectRatio(_$img);
                    _$img.fadeTo('fast', 1, function () {
                        //fade-in the semi-transparent article title
                        $(this).parent().prev().fadeIn();
                    });
                })
                .error(function () {
                    $(this).parent().parent().css({ "background": "none" });
                    $(this).parent().prev().fadeIn();
                })
                .attr("src", p_newsItem.Image.Url)
                .attr("alt", p_newsItem.Image.AltText || "")
                .end().end() //pop two times to go back to root
                .css({ "left": p_pos.left, "top": p_pos.top })
                .appendTo(p_containerSelector);
        }

        function adjustAspectRatio(p_img) {
            var _displayWidth = 300;
            var _displayHeight = 150;

            var _imgWidth = p_img.width();
            var _imgHeight = p_img.height();
            if (_imgWidth > _displayWidth) {
                var _adjustedHeight = Math.ceil((_displayWidth * _imgHeight) / (_imgWidth * 1.0));
                if (_adjustedHeight > _displayHeight) {
                    p_img.css({ "width": _displayWidth, "height": _adjustedHeight });
                }
            }
        }

    }
})();

(function () {
    var w = window;

    w.$TopNavView = function () {
        var _this = this;
        var _model = arguments[0];
        var _selectorId = arguments[1];
        var _eventCb = arguments[2];
        var _categories = _model.getList();
        var _menuTemplate = $("#topmenuTemplate").html();
        var _activeCategory = $Config.DefaultCtg;

        _this.render = function () {
            $.each(_categories, function (p_index, p_item) {
                $(_menuTemplate).find("a")
                               .text(p_item)
                               .bind("click.menu", function () {
                                   $(_selectorId + " li[data-cat=" + _activeCategory + "]").removeClass("sel");
                                   var _li = $(this).parent();
                                   _activeCategory = _li.addClass("sel").attr("data-cat");

                                   // execute the callback
                                   if (_eventCb["onchange"]) {
                                       _eventCb["onchange"].call(this, _activeCategory);
                                   }
                               })
                               .end()
                               .attr("data-cat", p_item)
                               .addClass(_activeCategory == p_item ? "sel" : "")
                               .appendTo(_selectorId);
            });
        }

        _this.dispose = function () {
            // unbind the event handlers
            $(_selectorId).find("a").unbind(".menu");
        }
    }
})();


(function () {
    var w = window;

    w.$NewsDataModel = function () {
        ///<summary>Model for the news data</summary>

        var _this = this;
        var _newsFeed = arguments[0] || {};

        _this.yieldArticles = function (p_category, p_numOfArticles) {
            ///<summary>Returns the new unprocessed articles for a particular category</summary>
            var _returnData = [];

            var _newsItems = _newsFeed.News[p_category] || [];
            var _total = 0;

            if (!p_numOfArticles) {
                p_numOfArticles = (_newsItems.length ? _newsItems.length : 0);
            }

            //then slice the delta from the news items array (javascript safely handles the outofbound index arguments)
            if (p_numOfArticles > 0) {
                var _sliceStartIndex = 0;
                var _sliceEndIndex = _sliceStartIndex + p_numOfArticles - _total;

                _returnData = _newsItems.slice(_sliceStartIndex, _sliceEndIndex);
            }

            return _returnData;
        }

    }
})();


(function () {
    var w = window;

    w.$CategoriesModel = function () {
        ///<summary>Model for the list of categories (like Headlines, Sports etc)</summary>        
        var _this = this;

        //capture the categories in the first argument
        var _categories = arguments[0] || [];

        _this.getList = function () {
            return _categories;
        }
    }
})();


(function () {
    var w = window;

    var _prefix;
    if ($.browser.chrome) {
        _prefix = "webkit";
    }
    else ($.browser.mozilla)
    {
        _prefix = "moz";
    }

    w.$AnimHelper = new function () {
        var _this = this;
        _this.shrinkAndSlide = function (p_selector) {
            var _def = new $.Deferred();
            var _container = $(p_selector);
            var _animClass = ((Math.random() * 50) << 0) % 2 == 0 ? "shrink_slideleft" : "shrink_slideright"
            // if unsupported browser or zero/multiple containers then just return resolved promise
            // currently supporting animation on only one container
            if (!_prefix || _container.length != 1) {
                _def.resolve();
                return _def.promise();
            }

            var _onAnimEnd = function () {
                if (!_def.resolved) {
                    _container.unbind(".anim").removeClass(_animClass);
                    _def.resolved = true;
                    _def.resolve();
                }
            }
            _container.bind(_prefix + "AnimationEnd.anim", function () {
                _onAnimEnd();
            });

            _container.addClass(_animClass);

            //fallback incase if browser didn't call the animationend
            setTimeout(function () { _onAnimEnd(); }, 550);

            return _def.promise();
        }
    }
})();
