﻿var updatedRow;
var table;
var datatable;
var exportedCols = [];
function showSuccessMessage(message = 'Saved successfully!')
{
	Swal.fire({
		icon: "success",
		title: "Success",
		text: message,
		customClass: {
			confirmButton: "btn btn-primary"
		}
	});
}
function showErrorMessage(message = 'Something went wrong!') {
	Swal.fire({
		icon: "error",
		title: "Oops...",
		text: message.responseText !== undefined ? message.responseText :message,
		customClass: {
			confirmButton: "btn btn-primary"
		}
	});
}
function disableSubmitButton(btn) {
	$(btn).attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}
function onModalBegin() {
	disableSubmitButton($('#Modal').find(':submit'));
}
function onModalSuccess(row)
{
	$('#Modal').modal('hide');
	showSuccessMessage();
	if (updatedRow !== undefined) {
		datatable.row(updatedRow).remove().draw();
		updatedRow = undefined;
	}
	var newRow = $(row);
	datatable.row.add(newRow).draw();
	KTMenu.init();
	KTMenu.initHandlers();
}
function onModalComplete() {
  $(':submit').removeAttr('disabled').removeAttr('data-kt-indicator');
}
//select2
function applySelect2() {
	$('.js-select2').select2();
	$('.js-select2').on('select2:select', function (e) {
		$('form').not('#SignOut').validate().element('#' + $(this).attr('id'));
	});
}
//DataTables
//exported Cols
var headers = $('th');
$.each(headers, function (i) {
	if (!$(this).hasClass('js-no-export')) 
		exportedCols.push(i);
	
});
// Class definition
var KTDatatables = function () {
	// Private functions
	var initDatatable = function () {


		// Init datatable --- more info on datatables: https://datatables.net/manual/
		datatable = $(table).DataTable({
			"info": false,
			'pageLength': 10,
		});
	}
	// Hook export buttons
	var exportButtons = () => {
		const documentTitle = $('.js-datatables').data('documnent-title');
		var buttons = new $.fn.dataTable.Buttons(table, {
			buttons: [
				{
					extend: 'copyHtml5',
					title: documentTitle,
					exportOptions: {
						columns: exportedCols
					}
				},
				{
					extend: 'excelHtml5',
					title: documentTitle,
					exportOptions: {
						columns: exportedCols
					}
				},
				{
					extend: 'csvHtml5',
					title: documentTitle,
					exportOptions: {
						columns: exportedCols
					}
				},
				{
					extend: 'pdfHtml5',
					title: documentTitle,
					exportOptions: {
						columns: exportedCols
					},
					customize: function (doc) {
						pdfMake.fonts = {
							Arial: {
								normal: 'arial',
								bold: 'arial',
								italics: 'arial',
								bolditalics: 'arial'
							}
						}
						doc.defaultStyle.font = 'Arial';
					}
				}
			]
		}).container().appendTo($('#kt_datatable_example_buttons'));

		// Hook dropdown menu click event to datatable export buttons
		const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
		exportButtons.forEach(exportButton => {
			exportButton.addEventListener('click', e => {
				e.preventDefault();

				// Get clicked export value
				const exportValue = e.target.getAttribute('data-kt-export');
				const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

				// Trigger click event on hidden datatable export buttons
				target.click();
			});
		});
	}

	// Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
	var handleSearchDatatable = () => {
		const filterSearch = document.querySelector('[data-kt-filter="search"]');
		filterSearch.addEventListener('keyup', function (e) {
			datatable.search(e.target.value).draw();
		});
	}

	// Public methods
	return {
		init: function () {
			table = document.querySelector('.js-datatables');

			if (!table) {
				return;
			}

			initDatatable();
			exportButtons();
			handleSearchDatatable();
		}
	};
}();

$(document).ready(function () {
	// disable submit button
	$('form').not('#SignOut').not('.js-exclude-validation').on('submit', function () {
		if ($('.js-tinymce').length > 0) {
			$('.js-tinymce').each(function () {
				var input = $(this);
				var content = tinyMCE.get(input.attr('id')).getContent();
				input.val(content);
			});

		}
		if ($(this).valid()) disableSubmitButton($(this).find(':submit'));
	});
	//TinyMce
	if ($('.js-tinymce').length > 0)
	{
		var options = { selector: ".js-tinymce", height: "422" };

		if (KTThemeMode.getMode() === "dark") {
			options["skin"] = "oxide-dark";
			options["content_css"] = "dark";
		}

		tinymce.init(options);
	}

	//select2
	applySelect2();

	//Date picker
	$('.js-datepicker').daterangepicker({
		singleDatePicker: true,
		autoApply: true,
		drops:'up',
		maxDate:new Date()
	});

	// Sweet Alert
    var message = $('#Message').text();
    if (message !== '') {
		showSuccessMessage(message);
	}
	//Data tables
	KTUtil.onDOMContentLoaded(function () {
		KTDatatables.init();
	});

	// handle bootstrap modal
	$('body').on('click', '.js-render-modal', function () {
		var btn = $(this);
		var modal = $('#Modal');
		modal.find('#ModalLabel').text(btn.data('title'));
		if (btn.data('update') !== undefined) {
			updatedRow = btn.parents('tr');
		}
		$.get({
			url: btn.data('url'),
			success: function (form) {
				modal.find('.modal-body').html(form);
				$.validator.unobtrusive.parse(modal);
				applySelect2();
			},
			error: function () {
				showErrorMessage();
			}
		});
		modal.modal('show');
	});
	//Handel toggle status
	$('body').on('click', '.js-toggle-status', function () {
		var btn = $(this);
		bootbox.confirm({
			message: 'are you sure that you need to toggle this item status?',
			buttons: {
				confirm: {
					label: 'Yes',
					className: 'btn-danger'
				},
				cancel: {
					label: 'No',
					className: 'btn-secondary'
				}
			},
			callback: function (result) {
				if (result) {
					$.post({
						url: btn.data('url'),
						data: {
							'__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
						},
						success: function (lastUpdatedOn) {
							var row = btn.parents('tr');
							var status = row.find('.js-status');
							var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
							status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
							row.find('.js-updated-on').html(lastUpdatedOn);
							showSuccessMessage();
							row.addClass('animate__animated animate__flash');
						},
						error: function () {
							showErrorMessage();
						}
					});
				}
			}
		});
	});
	//handle Confirm 
	$('body').on('click', '.js-confirm', function () {
		var btn = $(this);
		bootbox.confirm({
			message:btn.data('message'),
			buttons: {
				confirm: {
					label: 'Yes',
					className: 'btn-success'
				},
				cancel: {
					label: 'No',
					className: 'btn-secondary'
				}
			},
			callback: function (result) {
				if (result) {
					$.post({
						url: btn.data('url'),
						data: {
							'__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
						},
						success: function () {
							showSuccessMessage();
						},
						error: function () {
							showErrorMessage();
						}
					});
				}
			}
		});
	});
	//handle signout
	$('.js-signout').on('click', function () {
		$('#SignOut').submit();
	});
});
