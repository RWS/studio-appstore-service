class DropDown {
    #toggler;
    #menu;
    #aggregator;
    #productsSummary;
    #overflowArrows;
    #filters;
    #isReadOnly;

    constructor(toggler, menu, aggregator, productsSummary, overflowArrows, filters, isReadOnly) {
        this.#toggler = toggler;
        this.#menu = menu;
        this.#aggregator = aggregator;
        this.#productsSummary = productsSummary;
        this.#overflowArrows = overflowArrows;
        this.#filters = filters;
        this.#isReadOnly = isReadOnly;
    }

    Init() {
        if (!this.#isReadOnly) {
            this.#SortOptions(this.#menu, Array.from(this.#menu.querySelectorAll("li")).find(p => p.hasAttribute("selected")));
            this.#CreateCollapsedSummary(true);
            this.#menu.addEventListener('click', e => this.#OptionClickEventListener(e))
            this.#overflowArrows.forEach(arrow => arrow.addEventListener('mouseenter', () => this.#EraseMouseEnterEventListener()))
            this.#overflowArrows.forEach(arrow => arrow.addEventListener('mouseleave', () => this.#EraseMouseLeaveEventListener()))

            var observer = new MutationObserver(mutations => {
                mutations.forEach(mutation => {
                    if (mutation.attributeName == "aria-expanded") {
                        if (mutation.target.ariaExpanded == "true") {
                            this.#CreateCollapsedSummary(false);
                            return;
                        }

                        this.#CreateCollapsedSummary(true);
                    }
                });
            });

            observer.observe(this.#toggler, {
                attributes: true,
                attributeFilter: ['aria-expanded']
            });
        }

        this.#overflowArrows.forEach(arrow => arrow.addEventListener('click', e => this.#OverflowArrowEventListener(e)))
        this.#TryToggleOverflowArrows();
    }

    #CreateCollapsedSummary(isCollapsed) {
        if (this.#aggregator.val().length == 0) {
            this.#productsSummary.innerHTML = "Select...";
            return;
        }

        this.#productsSummary.innerHTML = "";
        this.#menu.querySelectorAll("li").forEach(option => {
            this.#aggregator.val().forEach((val, i) => {
                if (option.id == val) {
                    this.#CreateProductBox(option, isCollapsed, this.#aggregator.val().length == (i + 1));
                }
            })
        })
    }

    #CreateProductBox(product, isCollapsed, isLast) {
        var box = document.createElement('span');
        box.id = product.id;
        var cross = document.createElement('i');
        if (isCollapsed) {
            if (!isLast) {
                box.classList.add("border-0", "rounded", "px-1", "selection-box", "d-flex", "me-2");
                cross.classList.add('fa', 'fa-circle', "align-self-center", "ms-2", 'p-1');
                cross.style.fontSize = "8px";
            }

        } else {
            cross.classList.add('fa', 'fa-times', "align-self-center", "ms-2", 'cursor-pointer', 'p-1');
            cross.ariaLabel = `Remove ${product.innerText} from selection`;
            cross.setAttribute("role", "button")
            box.classList.add("border", "rounded", "px-1", "selection-box", "d-flex", "me-2", "bg-light");
            cross.addEventListener('click', e => this.#EraseMouseClickEventListener(e))
            cross.addEventListener('mouseenter', e => this.#EraseMouseEnterEventListener(e));
            cross.addEventListener('mouseleave', e => this.#EraseMouseLeaveEventListener(e))
        }

        var text = document.createElement('p');
        text.classList.add('m-0', 'align-middle', 'text-nowrap');
        text.innerText = product.innerText;
        box.append(text, cross);
        this.#productsSummary.append(box);
        this.#TryToggleOverflowArrows();
    }

    #TryToggleOverflowArrows() {
        if (this.#productsSummary.offsetWidth < this.#productsSummary.parentElement.offsetWidth) {
            this.#overflowArrows.forEach(arrow => arrow.classList.add("d-none"));
            return;
        }

        this.#overflowArrows.forEach(arrow => arrow.classList.remove("d-none"));
    }

    #EraseMouseClickEventListener(e) {
        this.#UpdateOnErase(e.target.parentElement.id);
        e.target.parentElement.remove();
        this.#toggler.setAttribute("data-bs-toggle", "dropdown");
        this.#TryToggleOverflowArrows();
    }

    #EraseMouseEnterEventListener(e) {
        this.#toggler.removeAttribute("data-bs-toggle");
        if (e) {
            e.target.classList.add("bg-white")
        }
    }

    #EraseMouseLeaveEventListener(e) {
        this.#toggler.setAttribute("data-bs-toggle", "dropdown");
        if (e) {
            e.target.classList.remove("bg-white")
        }
    }

    #OverflowArrowEventListener(e) {
        if (e.target.classList.contains('fa-chevron-right')) {
            this.#productsSummary.scrollLeft += 25;
            return;
        }

        this.#productsSummary.scrollLeft -= 25;
    }

    #OptionClickEventListener(e) {
        if (e.target.hasAttribute("selected")) {
            e.target.removeAttribute("selected");
            e.target.selected = false;
            this.#UpdateSelectedOptions()
            this.#RemoveProductBox(e.target);
            return;
        }

        e.target.setAttribute("selected", "selected");
        e.target.selected = true;
        this.#SortOptions(e.target.parentElement, e.target);
        this.#UpdateSelectedOptions()
        this.#CreateCollapsedSummary(false)
    }

    #RemoveProductBox(option) {
        var product = Array.from(this.#productsSummary.querySelectorAll('span')).find(x => x.id == option.id);

        if (product) {
            this.#productsSummary.removeChild(product);
        }

        this.#TryToggleOverflowArrows();
        if (this.#aggregator.val().length == 0) {
            this.#productsSummary.innerHTML = "Select...";
            return;
        }
    }

    #SortOptions(select, product) {
        if (!product) {
            return;
        }

        select.querySelectorAll("li").forEach(option => {
            this.#filters.forEach(filter => {
                if (product.innerText.includes(filter)) {
                    if (!option.innerText.includes(filter)) {
                        option.hidden = true;
                        option.removeAttribute("selected")
                    }
                }
            })
        })
    }

    #UpdateSelectedOptions() {
        var selected = Array.from(this.#menu.querySelectorAll('li')).filter(option => option.hasAttribute("selected")).map(option => option.id);
        this.#aggregator.val(selected)
        if (this.#aggregator.val().length == 0) {
            this.#menu.querySelectorAll('li').forEach(option => {
                option.hidden = false;
            });
            this.#productsSummary.innerHTML = "Select...";
        }
    }

    #UpdateOnErase(id) {
        let options = Array.from(this.#menu.querySelectorAll('li'));
        options.find(o => o.id == id).removeAttribute("selected");
        this.#UpdateSelectedOptions()
    }
}