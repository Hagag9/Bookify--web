﻿var currentCopies = [];
var selectedCopies = [];
var isEditMode = false;

$(document).ready(function () {
    if ($('.js-copy').length > 0) {
        perepareInputs();
        currentCopies = selectedCopies;
        isEditMode = true;
    }
    $('.js-search').on('click', function (e) {
        e.preventDefault();

        var serial = $('#Value').val();

        if (selectedCopies.find(c => c.serial == serial))
        {
            showErrorMessage('You cannot add the same copy');
            return;
        }
        if (selectedCopies.length >= maxAllowedCopies) {
            showErrorMessage(`You cannot add more than ${maxAllowedCopies} book(s)`);
            return;
        }
        $('#SearchForm').submit();
    });
    $('body').on('click', '.js-remove', function () {
        var btn = $(this);
        var container = btn.parents('.js-copy-container');

        if (isEditMode) {
            btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Re-Add');
            container.find('img').css('opacity', '0.5');
            container.find('h4').css('text-decoration', 'line-through');
            container.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name').removeAttr('id');
        }
        else {
            container.remove();
        }
        
        perepareInputs();  

        if (selectedCopies.length == 0 || JSON.stringify(currentCopies) == JSON.stringify(selectedCopies)) 
            $('#CopiesForm').find(':submit').addClass('d-none');
        else 
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });

    $('body').on('click', '.js-readd', function () {
        var btn = $(this);
        var container = btn.parents('.js-copy-container');

        btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Remove');
        container.find('img').css('opacity', '1');
        container.find('h4').css('text-decoration', 'none');
        container.find('.js-removed').toggleClass('js-copy js-removed');

        perepareInputs();

        if (JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });
});
function OnAddCopySuccess(copy)
{
    $('#Value').val('');
    var bookId = $(copy).find('.js-copy').data('book-id');
    if (selectedCopies.find(c => c.bookId == bookId))
    {
        showErrorMessage('You cannot add more than  one copy fot the same book');
        return
    }
 
    $('#CopiesForm').prepend(copy);
    $('#CopiesForm').find(':submit').removeClass('d-none');
    perepareInputs();  
}
function perepareInputs() {
    var copies = $('.js-copy');
    selectedCopies = [];
    $.each(copies, function (i, input) {
        var $input = $(input);

        selectedCopies.push({ serial: $input.val(), bookId: $input.data('book-id') });
        $input.attr('name', `SelectedCopies[${i}]`).attr('id', `SelectedCopies_${i}_`)
    });
}