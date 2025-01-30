const UserSearch = {
    initialise: function() {
        $(document).ready(() => {
            $("#search-form").submit(e => {
                e.preventDefault();

                if (!$(e.target).valid()) {
                    return;
                }

                this.toggleLoadingState(false, true);

                // avoid flicker for fast (cached) responses
                const showLoadingTimeout = setTimeout(() => {
                    this.toggleLoadingState(true, true);
                }, 100);

                this.fetchSearchResult(e.target)
                    .always(() => {
                        clearTimeout(showLoadingTimeout);
                        this.toggleLoadingState(false, false);
                    });
            });

            const resultContainer = $("#search-result-container");
            $("#Username").on("input", () => {
                resultContainer.html("").removeClass("show");
            });
        });
    },

    toggleLoadingState: function(showLoading, disableInput) {
        const spinner = $(".user-search-submit-container .loading-spinner");
        const form = $("#search-form");
        const submitButton = $(".start-user-search");

        spinner.toggle(showLoading);
        submitButton.text(showLoading ? "" : "Search");

        if (disableInput) {
            submitButton.prop("disabled", true);
            form.prop("disabled", true);
        } else {
            submitButton.removeAttr("disabled");
            form.removeAttr("disabled");
        }
    },

    fetchSearchResult: function(form) {
        const resultContainer = $("#search-result-container");

        return $.ajax({
            url: form.action,
            method: form.method,
            data: $(form).serialize(),
        })
        .done(result => {
            resultContainer.html(result).addClass("show");
        })
        .fail(response => {
            if (response.status == 400) {
                return;
            }

            resultContainer
                .html("<div style='margin-top:3rem'>Sorry, an unknown error has occurred.<div>")
                .addClass("show");
        });
    }
};