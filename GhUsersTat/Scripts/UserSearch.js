const UserSearch = {
    initialise: function() {
        $(document).ready(() => {
            const resultContainer = $("#search-result-container");

            $("#search-form").submit(e => {
                e.preventDefault();

                this.toggleLoadingState(false, true);

                const showLoadingTimeout = setTimeout(() => {
                    this.toggleLoadingState(true, true);
                }, 300);

                $.ajax({
                    url: e.target.action,
                    method: e.target.method,
                    data: $(e.target).serialize(),
                    success: result => {
                        clearTimeout(showLoadingTimeout);
                        this.toggleLoadingState(false, false);

                        resultContainer.html(result);
                        resultContainer.addClass("show");
                    },
                    error: () => {
                        clearTimeout(showLoadingTimeout);
                        this.toggleLoadingState(false, false);

                        resultContainer.html("Sorry, an unknown error has occurred.");
                        resultContainer.addClass("show");
                    }
                });
            });

            $("#Username").on("input", () => {
                resultContainer.html("");
                resultContainer.removeClass("show");
            });
        });
    },

    toggleLoadingState: function(showLoading, disableInput) {
        const spinner = $(".user-search-submit-container .loading-spinner");
        const form = $("#search-form");
        const submitButton = $(".start-user-search");

        if (showLoading) {
            spinner.show();
            submitButton.text("");
        } else {
            spinner.hide();
            submitButton.text("Search");
        }

        if (disableInput) {
            submitButton.prop("disabled", true);
            form.prop("disabled", true);
        } else {
            submitButton.removeAttr("disabled");
            form.removeAttr("disabled");
        }
    }
};