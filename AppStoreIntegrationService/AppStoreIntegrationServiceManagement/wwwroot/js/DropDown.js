class DropDown {
    #toggler;
    #menu;
    #aggregator;
    #productsSummary;
    #overflowArrows;
    #dropdownContainer;
    #filters;

    constructor(toggler, menu, aggregator, productsSummary, overflowArrows, dropdownContainer, filters) {
        this.#toggler = toggler;
        this.#menu = menu;
        this.#aggregator = aggregator;
        this.#productsSummary = productsSummary;
        this.#overflowArrows = overflowArrows;
        this.#dropdownContainer = dropdownContainer;
        this.#filters = filters;
    }

    Init() {
        let productList = document.querySelector(`${this.#dropdownContainer} ${this.#menu}`);
        this.#SortOptions(productList, Array.from(productList.querySelectorAll("li")).find(p => p.hasAttribute("selected")));
        this.#CreateCollapsedSummary(true);
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).addEventListener('click', e => this.#OptionClickEventListener(e))
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).addEventListener('mouseenter', () => this.#EraseMouseEnterEventListener())
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).addEventListener('mouseleave', () => this.#EraseMouseLeaveEventListener())
        document.querySelectorAll(`${this.#dropdownContainer} ${this.#overflowArrows}`).forEach(arrow => arrow.addEventListener('click', e => this.#OverflowArrowEventListener(e)))
        document.querySelectorAll(`${this.#dropdownContainer} ${this.#overflowArrows}`).forEach(arrow => arrow.addEventListener('mouseenter', () => this.#EraseMouseEnterEventListener()))
        document.querySelectorAll(`${this.#dropdownContainer} ${this.#overflowArrows}`).forEach(arrow => arrow.addEventListener('mouseleave', () => this.#EraseMouseLeaveEventListener()))

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

        observer.observe(document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`), {
            attributes: true,
            attributeFilter: ['aria-expanded']
        });
    }

    #CreateCollapsedSummary(isCollapsed) {
        if ($(`${this.#dropdownContainer} ${this.#aggregator}`).val().length == 0) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "Select...";
            return;
        }

        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "";
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).querySelectorAll("li").forEach(option => {
            $(`${this.#dropdownContainer} ${this.#aggregator}`).val().forEach(val => {
                if (option.id == val) {
                    this.#CreateProductBox(option, isCollapsed);
                }
            })
        })
    }

    #CreateProductBox(product, isCollapsed) {
        var box = document.createElement('span');
        box.id = product.id;
        var cross = document.createElement('i');
        if (isCollapsed) {
            box.classList.add("border-0", "rounded", "px-1", "selection-box", "d-flex", "me-2");
            cross.classList.add('fa', 'fa-circle', "align-self-center", "ms-2", 'p-1');
            cross.style.fontSize = "8px";
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
        text.classList.add('m-0', 'align-middle');
        text.innerText = product.innerText;
        box.append(text, cross);
        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).append(box);
        this.#TryToggleOverflowArrows();
    }

    #TryToggleOverflowArrows() {
        var productsConatiner = document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`);
        if (productsConatiner.offsetWidth < productsConatiner.parentElement.offsetWidth) {
            document.querySelectorAll(`${this.#dropdownContainer} ${this.#overflowArrows}`).forEach(arrow => arrow.classList.add("d-none"));
            return;
        }

        document.querySelectorAll(`${this.#dropdownContainer} ${this.#overflowArrows}`).forEach(arrow => arrow.classList.remove("d-none"));
    }

    #EraseMouseClickEventListener(e) {
        this.#UpdateOnErase(e.target.parentElement.id);
        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).removeChild(e.target.parentElement);
        document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`).setAttribute("data-bs-toggle", "dropdown");
        this.#TryToggleOverflowArrows();
    }

    #EraseMouseEnterEventListener(e) {
        document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`).removeAttribute("data-bs-toggle");
        if (e) {
            e.target.classList.add("bg-white")
        }
    }

    #EraseMouseLeaveEventListener(e) {
        document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`).setAttribute("data-bs-toggle", "dropdown");
        if (e) {
            e.target.classList.remove("bg-white")
        }
    }

    #OverflowArrowEventListener(e) {
        if (e.target.classList.contains('fa-chevron-right')) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).scrollLeft += 25;
            return;
        }

        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).scrollLeft -= 25;
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
        var product = document.getElementById(option.id)
        if (product) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).removeChild(product);
        }
        
        this.#TryToggleOverflowArrows();
        if ($(`${this.#dropdownContainer} ${this.#aggregator}`).val().length == 0) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "Select...";
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
        var selected = Array.from(document.querySelectorAll(`${this.#menu} li`)).filter(option => option.hasAttribute("selected")).map(option => option.id);
        $(`${this.#dropdownContainer} ${this.#aggregator}`).val(selected)
        if ($(`${this.#dropdownContainer} ${this.#aggregator}`).val().length == 0) {
            document.querySelectorAll(`${this.#menu} li`).forEach(option => {
                option.hidden = false;
            });
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "Select...";
        }
    }

    #UpdateOnErase(id) {
        let options = Array.from(document.querySelectorAll(`${this.#dropdownContainer} ${this.#menu} li`));
        options.find(o => o.id == id).removeAttribute("selected");
        this.#UpdateSelectedOptions()
    }
}