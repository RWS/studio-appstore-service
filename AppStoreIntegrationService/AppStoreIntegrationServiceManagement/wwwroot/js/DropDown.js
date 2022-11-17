class DropDown {
    #dropdownContainer;
    #toggler;
    #menu;
    #productsSummary;
    #overflowArrows;

    constructor(toggler, menu, productsSummary, overflowArrows, dropdownContainer) {
        this.#dropdownContainer = dropdownContainer;
        this.#toggler = toggler;
        this.#menu = menu;
        this.#productsSummary = productsSummary;
        this.#overflowArrows = overflowArrows;
    }

    Init() {
        let productList = document.querySelector(`${this.#dropdownContainer} ${this.#menu}`);
        this.#SortOptions(productList, Array.from(productList.querySelectorAll("option")).find(p => p.hasAttribute("selected")));
        this.#CreateCollapsedSummary(true);
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).addEventListener('click', e => this.#OptionClickEventListener(e))
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
        if ($(`${this.#dropdownContainer} ${this.#menu}`).val().length == 0) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "Select products...";
            return;
        }

        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "";
        document.querySelector(`${this.#dropdownContainer} ${this.#menu}`).querySelectorAll("option").forEach(option => {
            $(`${this.#dropdownContainer} ${this.#menu}`).val().forEach(val => {
                if (option.value == val) {
                    this.#CreateProductBox(option, isCollapsed);
                }
            })
        })
    }

    #CreateProductBox(product, isCollapsed) {
        var box = document.createElement('span');
        box.classList.add(isCollapsed ? "border-0" : "border", "rounded", "px-1", "selection-box", "d-flex", "me-2");
        box.id = product.value;
        var cross = document.createElement('i');
        if (isCollapsed) {
            cross.classList.add('fa', 'fa-circle', "align-self-center", "ms-2", 'p-1');
            cross.style.fontSize = "8px";
        } else {
            cross.classList.add('fa', 'fa-times', "align-self-center", "ms-2", 'cursor-pointer', 'p-1');
            cross.addEventListener('click', e => this.#EraseMouseClickEventListener(e))
            cross.addEventListener('mouseenter', () => this.#EraseMouseEnterEventListener());
            cross.addEventListener('mouseleave', () => this.#EraseMouseLeaveEventListener())
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

    #EraseMouseEnterEventListener() {
        document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`).removeAttribute("data-bs-toggle");
    }

    #EraseMouseLeaveEventListener() {
        document.querySelector(`${this.#dropdownContainer} ${this.#toggler}`).setAttribute("data-bs-toggle", "dropdown");
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
            this.#UpdateSelectedOptions(e.target.parentElement)
            this.#RemoveProductBox(e.target);
            return;
        }

        e.target.setAttribute("selected", "selected");
        e.target.selected = true;
        this.#SortOptions(e.target.parentElement, e.target);
        this.#UpdateSelectedOptions(e.target.parentElement)
        this.#CreateCollapsedSummary(false)
    }

    #RemoveProductBox(product) {
        document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).removeChild(document.getElementById(product.value));
        this.#TryToggleOverflowArrows();
        if ($(`${this.#dropdownContainer} ${this.#menu}`).val().length == 0) {
            document.querySelector(`${this.#dropdownContainer} ${this.#productsSummary}`).innerHTML = "Select products...";
            return;
        }
    }

    #SortOptions(select, product) {
        if (!product) {
            return;
        }

        
        let productName = product.innerText;
        select.querySelectorAll("option").forEach(option => {
            if (productName.includes("Trados")) {
                if (!option.innerText.includes("Trados")) {
                    option.disabled = true;
                    option.removeAttribute("selected")
                }
            }

            if (productName.includes("Language")) {
                if (!option.innerText.includes("Language")) {
                    option.disabled = true;
                    option.removeAttribute("selected")
                }
            }

            if (productName.includes("Passolo")) {
                if (!option.innerText.includes("Passolo")) {
                    option.disabled = true;
                    option.removeAttribute("selected")
                }
            }

            if (productName.includes("Extract")) {
                if (!option.innerText.includes("Extract")) {
                    option.disabled = true;
                    option.removeAttribute("selected")
                }
            }
        })
    }

    #UpdateSelectedOptions(select) {
        var selected = [...select.options].filter(option => option.hasAttribute("selected")).map(option => option.value);
        $(`${this.#dropdownContainer} ${this.#menu}`).val(selected)
        if ($(`${this.#dropdownContainer} ${this.#menu}`).val().length == 0) {
            select.querySelectorAll("option").forEach(option => {
                option.disabled = false;
            });
        }
    }

    #UpdateOnErase(id) {
        let options = Array.from(document.querySelectorAll(`${this.#dropdownContainer} ${this.#menu} option`));
        options.find(o => o.value == id).removeAttribute("selected");
        this.#UpdateSelectedOptions(document.querySelector(`${this.#dropdownContainer} ${this.#menu}`))
    }
}