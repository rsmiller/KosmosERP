import { test, expect } from '../../fixtures/test';
import { fail, ok } from '../../mocks/envelope';
import { users } from '../../fixtures/users';
import { LoginPage } from '../../pages/login-page';
import { AppShell } from '../../pages/app-shell';

// These tests exercise the login flow itself, so start signed out.
test.use({ storageState: { cookies: [], origins: [] } });

const { auth, password } = users.admin;

test.describe('database login', () => {
  test('valid credentials sign in and land on the ERP home', async ({ page, api }) => {
    api.on('POST', '/User/AuthenticateUser', ok(auth));
    const login = new LoginPage(page);

    await login.goto();
    await login.login(auth.user.username, password);

    await expect(page).toHaveURL(/\/erp\/?$/);
    await expect(new AppShell(page).navLink('Customers')).toBeVisible();

    const request = await api.waitForRequest('POST', '/User/AuthenticateUser');
    expect(request.body).toEqual({ username: auth.user.username, password });
  });

  test('rejected credentials show an error and stay on the login page', async ({ page, api }) => {
    api.on('POST', '/User/AuthenticateUser', fail(-1, 'Invalid username or password'));
    const login = new LoginPage(page);

    await login.goto();
    await login.login(auth.user.username, 'wrong-password');

    await expect(page.getByText('The username or password is incorrect.')).toBeVisible();
    await expect(page).toHaveURL(/\/login\/database$/);
  });

  test('empty fields are validated before calling the API', async ({ page, api }) => {
    const login = new LoginPage(page);

    await login.goto();
    await login.signIn.click();

    await expect(page.getByText('Please enter a username and password')).toBeVisible();
    expect(api.requestsTo('POST', '/User/AuthenticateUser')).toHaveLength(0);
  });

  test('an unreachable API fails the sign in', async ({ page, api }) => {
    api.abort('POST', '/User/AuthenticateUser');
    const login = new LoginPage(page);

    await login.goto();
    await login.login(auth.user.username, password);

    // Regression for BUG-008: this used to say the password was wrong.
    await expect(page.getByText('Unable to reach the authentication service.')).toBeVisible();
    await expect(page.getByText('The username or password is incorrect.')).toBeHidden();
    await expect(page).toHaveURL(/\/login\/database$/);
  });

  test('the landing page leads to the username & password form', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: 'Username & Password' }).click();

    await expect(page).toHaveURL(/\/login\/database$/);
    await expect(new LoginPage(page).username).toBeVisible();
  });
});
