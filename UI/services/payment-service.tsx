import { PaymentDto, PaymentListDto, PaymentEditCommand, PaymentProviderGetDto, PaymentProviderCreateDto, CreateStripePaymentIntent, StripePaymentIntentStatusCommand, SavedPaymentMethodsDto, GetSavedPaymentMethodsCommand, CreateStripeNewCardIntentCommand, PaymentCardGetDto } from "@/models/payment-models";
import { PaymentCreateCommand, PaymentDeleteCommand, PaymentFindCommand } from "@/models/payment-models";
import axiosInstance from "./axios-instance";
import { ApiResponse, PagedApiResponse } from "@/models/base-models";

export const paymentService = {
	async get(paymentId: number, token: string): Promise<ApiResponse<PaymentDto>> {
		const response = await axiosInstance(token).get<ApiResponse<PaymentDto>>(`/api/v1/Payment/GetPayment/?id=${paymentId}`);
		return response.data;
	},

	async getByGuid(guid: string, token: string): Promise<ApiResponse<PaymentDto>> {
		const response = await axiosInstance(token).get<ApiResponse<PaymentDto>>(`/api/v1/Payment/GetPaymentByGuid/?guid=${guid}`);
		return response.data;
	},

	async find(paymentFindCommand: PaymentFindCommand, token: string, start: number = 1, resultCount: number = 20, sortOrder: string = "created_on-asc"): Promise<PagedApiResponse<PaymentListDto[]>> {
		const response = await axiosInstance(token).post<PagedApiResponse<PaymentListDto[]>>(`/api/v1/Payment/FindPayment?Start=${start}&ResultCount=${resultCount}&SortOrder=${sortOrder}`, paymentFindCommand);
		return response.data;
	},

	async create(paymentCreateCommand: PaymentCreateCommand, token: string): Promise<ApiResponse<PaymentDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentDto>>(`/api/v1/Payment/CreatePayment`, paymentCreateCommand);
		return response.data;
	},

	async update(paymentEditCommand: PaymentEditCommand, token: string): Promise<ApiResponse<PaymentDto>> {
		const response = await axiosInstance(token).put<ApiResponse<PaymentDto>>(`/api/v1/Payment/UpdatePayment`, paymentEditCommand);
		return response.data;
	},

	async delete(paymentDeleteCommand: PaymentDeleteCommand, token: string): Promise<ApiResponse<PaymentDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentDto>>(`/api/v1/Payment/DeletePayment`, paymentDeleteCommand);
		return response.data;
	},

	async getStripeSessionFromARInvoce(commandModel: CreateStripePaymentIntent, token: string): Promise<ApiResponse<PaymentProviderCreateDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentProviderCreateDto>>(`/api/v1/Payment/GetStripePaymentIntentFromARInvoce`, commandModel);
		return response.data;
	},

	async getStripeSessionStatus(commandModel: StripePaymentIntentStatusCommand, token: string): Promise<ApiResponse<PaymentProviderGetDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentProviderGetDto>>(`/api/v1/Payment/GetStripePaymentIntentStatus`, commandModel);
		return response.data;
	},

	async getSavedPaymentMethods(commandModel: GetSavedPaymentMethodsCommand, token: string): Promise<ApiResponse<SavedPaymentMethodsDto>> {
		const response = await axiosInstance(token).post<ApiResponse<SavedPaymentMethodsDto>>(`/api/v1/Payment/GetSavedPaymentMethods`, commandModel);
		return response.data;
	},

	async payStripePaymentIntent(commandModel: StripePaymentIntentStatusCommand, token: string): Promise<ApiResponse<PaymentProviderGetDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentProviderGetDto>>(`/api/v1/Payment/PayStripePaymentIntent`, commandModel);
		return response.data;
	},

	async createStripeNewCardIntent(commandModel: CreateStripeNewCardIntentCommand, token: string): Promise<ApiResponse<PaymentCardGetDto>> {
		const response = await axiosInstance(token).post<ApiResponse<PaymentCardGetDto>>(`/api/v1/Payment/CreateStripeNewCard`, commandModel);
		return response.data;
	},
};
