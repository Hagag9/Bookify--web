function onAddCopySuccess(row) {
	$('#Modal').modal('hide');
	showSuccessMessage();
	$('tbody').prepend(row);
	KTMenu.createInstances();

	var count = $('#CopiesCount');
	var newCount = parseInt(count.text()) + 1;
	count.text(newCount);

	$('.js-alert').addClass('d-none');
	$('table').removeClass('d-none');

}
function onEditCopySuccess(row) {
	$('#Modal').modal('hide');
	showSuccessMessage();
	$(updatedRow).replaceWith(row);
	KTMenu.createInstances();

}