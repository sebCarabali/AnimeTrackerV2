/** @type {import('@commitlint/types').UserConfig} */
export default {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'scope-enum': [
      2,
      'always',
      ['spa', 'bff', 'ext', 'core', 'contracts', 'infra', 'ci', 'docs'],
    ],
  },
};
